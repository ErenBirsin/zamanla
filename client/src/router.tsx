import { createBrowserRouter, Navigate } from 'react-router'
import { AuthLayout } from '@/features/auth/AuthLayout'
import { PanelLayout } from '@/features/panel/PanelLayout'
import { GuestOnly, RequireAuth } from '@/shared/auth/guards'

export const router = createBrowserRouter([
  {
    // ilk açılışta lazy sayfa yüklenene kadar boş zemin gösterilir
    hydrateFallbackElement: <div className="min-h-dvh bg-sand" />,
    children: [
      { path: '/', element: <Navigate to="/app" replace /> },
      {
        element: <GuestOnly />,
        children: [
          {
            element: <AuthLayout />,
            children: [
              {
                path: '/login',
                lazy: async () => ({
                  Component: (await import('@/features/auth/LoginPage')).LoginPage,
                }),
              },
              {
                path: '/register',
                lazy: async () => ({
                  Component: (await import('@/features/auth/RegisterPage')).RegisterPage,
                }),
              },
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
            children: [
              {
                index: true,
                lazy: async () => ({
                  Component: (await import('@/features/dashboard/DashboardPage')).DashboardPage,
                }),
              },
              {
                path: 'calendar',
                lazy: async () => ({
                  Component: (await import('@/features/calendar/CalendarPage')).CalendarPage,
                }),
              },
              {
                path: 'services',
                lazy: async () => ({
                  Component: (await import('@/features/services/ServicesPage')).ServicesPage,
                }),
              },
              {
                path: 'staff',
                lazy: async () => ({
                  Component: (await import('@/features/staff/StaffPage')).StaffPage,
                }),
              },
              {
                path: 'customers',
                lazy: async () => ({
                  Component: (await import('@/features/customers/CustomersPage')).CustomersPage,
                }),
              },
            ],
          },
        ],
      },
      {
        path: '/b/:slug',
        lazy: async () => ({
          Component: (await import('@/features/booking/BookingPage')).BookingPage,
        }),
      },
      {
        path: '/b/:slug/r/:token',
        lazy: async () => ({
          Component: (await import('@/features/booking/ManagePage')).ManagePage,
        }),
      },
      {
        path: '*',
        lazy: async () => ({
          Component: (await import('@/features/system/NotFoundPage')).NotFoundPage,
        }),
      },
    ],
  },
])
