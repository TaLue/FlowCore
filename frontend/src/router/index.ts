import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import AppLayout from '@/layouts/AppLayout.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/LoginView.vue'),
    },
    {
      path: '/',
      component: AppLayout,
      meta: { requiresAuth: true },
      children: [
        {
          path: '',
          name: 'dashboard',
          component: () => import('@/views/DashboardView.vue'),
        },
        {
          path: 'requests',
          name: 'requests',
          component: () => import('@/views/requests/RequestListView.vue'),
        },
        {
          path: 'requests/:id',
          name: 'request-detail',
          component: () => import('@/views/requests/RequestDetailView.vue'),
        },
        {
          path: 'approvals',
          name: 'approvals',
          component: () => import('@/views/ApprovalsView.vue'),
        },
        {
          path: 'workflows',
          name: 'workflows',
          component: () => import('@/views/WorkflowsView.vue'),
        },
        {
          path: 'admin/users',
          name: 'admin-users',
          component: () => import('@/views/admin/UsersView.vue'),
          meta: { requiresAdmin: true },
        },
        {
          path: 'admin/departments',
          name: 'admin-departments',
          component: () => import('@/views/admin/DepartmentsView.vue'),
          meta: { requiresAdmin: true },
        },
      ],
    },
  ],
})

router.beforeEach((to) => {
  const auth = useAuthStore()
  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    return '/login'
  }
  if (to.path === '/login' && auth.isAuthenticated) {
    return '/'
  }
  if (to.meta.requiresAdmin && !auth.isAdmin) {
    return '/'
  }
})

export default router
