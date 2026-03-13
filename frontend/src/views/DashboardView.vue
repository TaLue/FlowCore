<template>
  <div class="p-6 lg:p-8 space-y-6">

    <!-- Header -->
    <div class="flex items-start justify-between">
      <div>
        <h1 class="text-2xl font-bold text-gray-900">Dashboard</h1>
        <p class="text-gray-500 mt-1">
          ยินดีต้อนรับกลับมา, <span class="font-medium text-gray-700">{{ auth.user?.username }}</span>
          &nbsp;·&nbsp;{{ todayTH }}
        </p>
      </div>
      <RouterLink
        to="/requests"
        class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium rounded-lg transition-colors shrink-0"
      >
        + สร้างคำขอใหม่
      </RouterLink>
    </div>

    <!-- Stat Cards -->
    <div class="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-4">
      <div
        v-for="card in statCards"
        :key="card.label"
        class="bg-white rounded-xl border border-gray-200 p-5 flex flex-col gap-3"
      >
        <div class="flex items-center justify-between">
          <span class="text-xs font-medium text-gray-500">{{ card.label }}</span>
          <span :class="card.iconBg" class="w-8 h-8 rounded-lg flex items-center justify-center text-base">
            {{ card.icon }}
          </span>
        </div>
        <p :class="card.valueColor" class="text-3xl font-bold">{{ card.value }}</p>
      </div>
    </div>

    <!-- Main content grid -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">

      <!-- Recent Requests (2/3 width) -->
      <div class="lg:col-span-2 bg-white rounded-xl border border-gray-200">
        <div class="flex items-center justify-between px-6 py-4 border-b border-gray-100">
          <h2 class="text-sm font-semibold text-gray-900">คำขอล่าสุด</h2>
          <RouterLink to="/requests" class="text-xs text-blue-600 hover:text-blue-800 font-medium">
            ดูทั้งหมด →
          </RouterLink>
        </div>
        <div v-if="loadingRequests" class="p-8 text-center text-gray-400 text-sm">กำลังโหลด...</div>
        <div v-else-if="recentRequests.length === 0" class="p-8 text-center text-gray-400 text-sm">
          ยังไม่มีคำขอ
        </div>
        <div v-else class="divide-y divide-gray-50">
          <RouterLink
            v-for="req in recentRequests"
            :key="req.id"
            :to="'/requests/' + req.id"
            class="flex items-center gap-4 px-6 py-3.5 hover:bg-gray-50 transition-colors"
          >
            <div class="min-w-0 flex-1">
              <p class="text-sm font-medium text-gray-900 truncate">{{ req.title }}</p>
              <p class="text-xs text-gray-400 mt-0.5">{{ req.requestTypeName }} · {{ formatDate(req.createdAt) }}</p>
            </div>
            <span :class="statusClass(req.status)" class="px-2 py-1 rounded-full text-xs font-medium shrink-0">
              {{ statusLabel(req.status) }}
            </span>
          </RouterLink>
        </div>
      </div>

      <!-- Pending Approvals (1/3 width) -->
      <div class="bg-white rounded-xl border border-gray-200">
        <div class="flex items-center justify-between px-6 py-4 border-b border-gray-100">
          <h2 class="text-sm font-semibold text-gray-900">รออนุมัติจากฉัน</h2>
          <RouterLink to="/approvals" class="text-xs text-blue-600 hover:text-blue-800 font-medium">
            ดูทั้งหมด →
          </RouterLink>
        </div>
        <div v-if="loadingApprovals" class="p-8 text-center text-gray-400 text-sm">กำลังโหลด...</div>
        <div v-else-if="pendingApprovals.length === 0" class="p-8 text-center text-gray-400 text-sm">
          ไม่มีรายการรออนุมัติ 🎉
        </div>
        <div v-else class="divide-y divide-gray-50">
          <div
            v-for="item in pendingApprovals"
            :key="item.approvalId"
            class="px-6 py-3.5 space-y-2"
          >
            <div>
              <p class="text-sm font-medium text-gray-900 truncate">{{ item.requestTitle }}</p>
              <p class="text-xs text-gray-400 mt-0.5">
                {{ item.requesterName }} · {{ item.requestTypeName }}
              </p>
            </div>
            <div class="flex gap-2">
              <button
                @click.stop="quickApprove(item.approvalId)"
                :disabled="actionLoading === item.approvalId"
                class="flex-1 py-1 bg-green-600 hover:bg-green-700 disabled:opacity-50 text-white text-xs font-medium rounded-md transition-colors"
              >
                อนุมัติ
              </button>
              <button
                @click.stop="quickReject(item.approvalId)"
                :disabled="actionLoading === item.approvalId"
                class="flex-1 py-1 bg-red-500 hover:bg-red-600 disabled:opacity-50 text-white text-xs font-medium rounded-md transition-colors"
              >
                ปฏิเสธ
              </button>
            </div>
          </div>
        </div>
      </div>

    </div>

    <!-- Quick Links (Admin) -->
    <div v-if="auth.isAdmin" class="bg-white rounded-xl border border-gray-200 p-5">
      <h2 class="text-sm font-semibold text-gray-900 mb-3">จัดการระบบ</h2>
      <div class="flex flex-wrap gap-3">
        <RouterLink
          to="/admin/users"
          class="flex items-center gap-2 px-4 py-2 border border-gray-200 hover:bg-gray-50 rounded-lg text-sm text-gray-700 font-medium transition-colors"
        >
          👥 จัดการผู้ใช้งาน
        </RouterLink>
        <RouterLink
          to="/admin/departments"
          class="flex items-center gap-2 px-4 py-2 border border-gray-200 hover:bg-gray-50 rounded-lg text-sm text-gray-700 font-medium transition-colors"
        >
          🏢 จัดการแผนก
        </RouterLink>
        <RouterLink
          to="/workflows"
          class="flex items-center gap-2 px-4 py-2 border border-gray-200 hover:bg-gray-50 rounded-lg text-sm text-gray-700 font-medium transition-colors"
        >
          ⚙️ จัดการ Workflow
        </RouterLink>
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import client from '@/api/client'

const auth = useAuthStore()

const requests = ref<any[]>([])
const pendingApprovals = ref<any[]>([])
const loadingRequests = ref(true)
const loadingApprovals = ref(true)
const actionLoading = ref<number | null>(null)

// ---------- Date ----------
const todayTH = new Date().toLocaleDateString('th-TH', {
  weekday: 'long',
  year: 'numeric',
  month: 'long',
  day: 'numeric',
})

// ---------- Status ----------
const STATUS_MAP: Record<number, { label: string; cls: string }> = {
  0: { label: 'ร่าง',           cls: 'bg-gray-100 text-gray-600' },
  1: { label: 'รอดำเนินการ',    cls: 'bg-blue-100 text-blue-700' },
  2: { label: 'อนุมัติแล้ว',   cls: 'bg-green-100 text-green-700' },
  3: { label: 'ปฏิเสธ',        cls: 'bg-red-100 text-red-700' },
  4: { label: 'ส่งกลับแก้ไข', cls: 'bg-orange-100 text-orange-700' },
}

function statusClass(s: string | number) {
  return STATUS_MAP[Number(s)]?.cls ?? 'bg-gray-100 text-gray-600'
}
function statusLabel(s: string | number) {
  return STATUS_MAP[Number(s)]?.label ?? String(s)
}

// ---------- Stats ----------
const statCards = computed(() => [
  {
    label: 'คำขอทั้งหมด',
    value: requests.value.length,
    icon: '📋',
    iconBg: 'bg-blue-50',
    valueColor: 'text-gray-900',
  },
  {
    label: 'รอดำเนินการ',
    value: requests.value.filter((r) => Number(r.status) === 1).length,
    icon: '⏳',
    iconBg: 'bg-yellow-50',
    valueColor: 'text-yellow-600',
  },
  {
    label: 'อนุมัติแล้ว',
    value: requests.value.filter((r) => Number(r.status) === 2).length,
    icon: '✅',
    iconBg: 'bg-green-50',
    valueColor: 'text-green-600',
  },
  {
    label: 'ปฏิเสธ',
    value: requests.value.filter((r) => Number(r.status) === 3).length,
    icon: '❌',
    iconBg: 'bg-red-50',
    valueColor: 'text-red-600',
  },
  {
    label: 'ส่งกลับแก้ไข',
    value: requests.value.filter((r) => Number(r.status) === 4).length,
    icon: '↩️',
    iconBg: 'bg-orange-50',
    valueColor: 'text-orange-600',
  },
])

// ---------- Recent requests (5 latest) ----------
const recentRequests = computed(() =>
  [...requests.value]
    .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
    .slice(0, 5),
)

// ---------- Data load ----------
onMounted(async () => {
  await Promise.all([loadRequests(), loadApprovals()])
})

async function loadRequests() {
  loadingRequests.value = true
  try {
    const res = await client.get('/requests')
    requests.value = Array.isArray(res.data) ? res.data : (res.data?.value ?? [])
  } finally {
    loadingRequests.value = false
  }
}

async function loadApprovals() {
  loadingApprovals.value = true
  try {
    const res = await client.get('/approvals/pending')
    const data = Array.isArray(res.data) ? res.data : (res.data?.value ?? [])
    pendingApprovals.value = data.slice(0, 5)
  } finally {
    loadingApprovals.value = false
  }
}

// ---------- Quick approve/reject ----------
async function quickApprove(id: number) {
  actionLoading.value = id
  try {
    await client.post(`/approvals/${id}/approve`, { comment: '' })
    await Promise.all([loadRequests(), loadApprovals()])
  } finally {
    actionLoading.value = null
  }
}

async function quickReject(id: number) {
  actionLoading.value = id
  try {
    await client.post(`/approvals/${id}/reject`, { comment: '' })
    await Promise.all([loadRequests(), loadApprovals()])
  } finally {
    actionLoading.value = null
  }
}

function formatDate(date: string) {
  return new Date(date).toLocaleDateString('th-TH', { day: 'numeric', month: 'short', year: 'numeric' })
}
</script>
