import { createBrowserRouter, Navigate } from 'react-router'
import { AuthLayout } from '@/features/auth/AuthLayout'
import { LoginPage } from '@/features/auth/LoginPage'
import { RegisterPage } from '@/features/auth/RegisterPage'
import { DashboardPage } from '@/features/dashboard/DashboardPage'
import { PanelLayout } from '@/features/panel/PanelLayout'
import { NotFoundPage } from '@/features/system/NotFoundPage'
import { GuestOnly, RequireAuth } from '@/shared/auth/guards'

export const router = createBrowserRouter([
  { path: '/', element: <Navigate to="/app" replace /> },
  {
    element: <GuestOnly />,
    children: [
      {
        element: <AuthLayout />,
        children: [
          { path: '/login', element: <LoginPage /> },
          { path: '/register', element: <RegisterPage /> },
        ],
      },
    ],
  },
  {
    path: '/app',
    element: <RequireAuth />,
    children: [
      {
        element: <PanelLayout />,
        children: [{ index: true, element: <DashboardPage /> }],
      },
    ],
  },
  { path: '*', element: <NotFoundPage /> },
])
