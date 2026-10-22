import { http } from '@/shared/api/http'
import type { AuthTokens } from '@/shared/models/auth'
import type { Result } from '@/shared/models/result'
import type { PublicInvitation } from './types'

export async function getInvitation(slug: string, token: string) {
  const { data } = await http.get<Result<PublicInvitation>>(`/public/${slug}/invitations/${token}`)
  return data.data!
}

// anahtar adresten gider, gövdede sadece şifre
export async function acceptInvitation(slug: string, token: string, password: string) {
  const { data } = await http.post<Result<AuthTokens>>(
    `/public/${slug}/invitations/${token}/accept`,
    { password },
  )
  return data.data!
}
