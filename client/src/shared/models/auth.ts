export interface AuthTokens {
  accessToken: string
  accessTokenExpiresAt: string
  refreshToken: string
  refreshTokenExpiresAt: string
}

export interface CurrentUser {
  id: string
  firstName: string
  lastName: string
  email: string
  role: 'Owner' | 'Staff'
  tenantId: string
  tenantName: string
  tenantSlug: string
}
