import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { toast } from 'sonner'
import { formatDateTime } from '@/features/calendar/time'
import type { Permission } from '@/shared/auth/permissions'
import { Dialog } from '@/shared/ui/Dialog'
import { FormError } from '@/shared/ui/FormError'
import { FormField } from '@/shared/ui/FormField'
import { type InviteValues, inviteSchema } from './staffAccountSchemas'
import {
  inviteStaff,
  updateAccountPermissions,
  updateAccountStatus,
  withdrawInvitation,
} from './staffAccountsApi'
import { accountBadgeClasses } from './staffOptions'
import type { InvitationCreated, StaffAccount } from './types'
import { staffAccountsQueryKey, usePermissionCatalog } from './useStaffAccounts'
import { confirmAction } from '@/shared/ui/confirm'

interface StaffAccountDialogProps {
  // null iken kapalı
  account: StaffAccount | null
  onClose: () => void
}

const secondaryButton =
  'h-11 cursor-pointer rounded-full border border-line px-5 text-sm font-semibold hover:bg-sand-soft disabled:cursor-default disabled:opacity-60'
const primaryButton =
  'h-11 cursor-pointer rounded-full bg-clay px-5 text-sm font-semibold text-white hover:bg-clay-dark disabled:cursor-default disabled:opacity-60'

export function StaffAccountDialog({ account, onClose }: StaffAccountDialogProps) {
  return (
    <Dialog
      open={account !== null}
      title={account ? `${account.staffFullName} · Panel hesabı` : 'Panel hesabı'}
      onClose={onClose}
    >
      {/* durum değişince form baştan kurulsun */}
      {account &&
        (account.userId ? (
          <AccountForm
            key={`${account.staffId}-${account.status}`}
            account={account}
            onClose={onClose}
          />
        ) : (
          <InviteForm
            key={`${account.staffId}-${account.status}`}
            account={account}
            onClose={onClose}
          />
        ))}
    </Dialog>
  )
}

// ---------------------------------------------------------------- yetki kutuları

interface PermissionListProps {
  value: Permission[]
  onChange: (value: Permission[]) => void
}

function PermissionList({ value, onChange }: PermissionListProps) {
  const { data: catalog = [] } = usePermissionCatalog()

  return (
    <fieldset className="flex flex-col gap-2">
      <legend className="mb-1.5 text-[13px] font-semibold">Yetkiler</legend>
      {catalog.map((permission) => (
        <label
          key={permission.value}
          className="flex cursor-pointer items-center gap-2.5 rounded-xl border border-line px-3 py-2.5 text-sm has-checked:border-clay has-checked:bg-clay-soft/40"
        >
          <input
            type="checkbox"
            className="size-4.5 accent-clay"
            checked={value.includes(permission.value)}
            onChange={(event) =>
              onChange(
                event.target.checked
                  ? [...value, permission.value]
                  : value.filter((item) => item !== permission.value),
              )
            }
          />
          {permission.label}
        </label>
      ))}
      <p className="text-[13px] text-muted">
        Yetki olmadan da takvimi görür, müşteri ekler ve kendi randevularını yönetir.
      </p>
    </fieldset>
  )
}

// ---------------------------------------------------------------- hesap yokken: davet

interface FormProps {
  account: StaffAccount
  onClose: () => void
}

function InviteForm({ account, onClose }: FormProps) {
  const queryClient = useQueryClient()
  const [created, setCreated] = useState<InvitationCreated | null>(null)
  const pending = account.status !== 'Hesap yok'

  const {
    register,
    handleSubmit,
    control,
    formState: { errors },
  } = useForm<InviteValues>({
    resolver: zodResolver(inviteSchema),
    defaultValues: { email: account.email ?? '', permissions: account.permissions },
  })

  const refresh = () => queryClient.invalidateQueries({ queryKey: staffAccountsQueryKey })

  const inviteMutation = useMutation({
    mutationFn: (values: InviteValues) =>
      inviteStaff(account.staffId, values.email, values.permissions),
    onSuccess: (result) => {
      setCreated(result)
      void refresh()
    },
    meta: { inlineError: true },
  })

  const withdrawMutation = useMutation({
    mutationFn: () => withdrawInvitation(account.staffId),
    onSuccess: (message) => {
      toast.success(message)
      void refresh()
      onClose()
    },
    meta: { inlineError: true },
  })

  // davet backend'de de reddedilir; formu hiç göstermeyelim
  if (!account.staffIsActive) {
    return (
      <>
        <p className="text-sm text-muted">
          Pasif personele davet gönderilemez. Önce personeli düzenleyip aktif yap.
        </p>
        <div className="flex justify-end">
          <button type="button" onClick={onClose} className={secondaryButton}>
            Kapat
          </button>
        </div>
      </>
    )
  }

  // link sahibe de gösterilir; e-posta gitmese de WhatsApp'tan iletilebilir
  if (created) {
    const copy = async () => {
      try {
        await navigator.clipboard.writeText(created.link)
        toast.success('Link kopyalandı')
      } catch {
        toast.error('Link kopyalanamadı, elle seçip kopyalayabilirsin')
      }
    }

    return (
      <>
        <p className="text-sm text-ink-soft">
          {created.emailSent
            ? 'Davet e-postası gönderildi. İstersen linki kendin de iletebilirsin.'
            : 'Davet oluşturuldu ama e-posta gönderilemedi. Linki personele kendin ilet.'}{' '}
          Link {formatDateTime(created.expiresAt)} tarihine kadar geçerli.
        </p>
        <div className="flex flex-wrap items-center gap-2">
          <code className="min-w-48 flex-1 truncate rounded-xl border border-line-soft bg-sand-soft px-3 py-2.5 text-sm">
            {created.link}
          </code>
          <button type="button" onClick={copy} className={secondaryButton}>
            Kopyala
          </button>
        </div>
        <div className="flex justify-end">
          <button type="button" onClick={onClose} className={primaryButton}>
            Tamam
          </button>
        </div>
      </>
    )
  }

  return (
    <form
      noValidate
      onSubmit={handleSubmit((values) => inviteMutation.mutate(values))}
      className="flex flex-col gap-4"
    >
      <p className="text-sm text-muted">
        {account.status === 'Davet gönderildi' &&
          `Davet ${formatDateTime(account.invitationExpiresAt!)} tarihine kadar geçerli. Yeniden gönderirsen eski link geçersiz olur.`}
        {account.status === 'Davetin süresi doldu' &&
          'Davetin süresi doldu. Yeniden göndererek yeni bir link oluşturabilirsin.'}
        {account.status === 'Hesap yok' &&
          'Personele bir link gönderilir; şifresini belirleyince panele girebilir.'}
      </p>

      <FormField
        label="E-posta"
        type="email"
        autoComplete="off"
        placeholder="personel@ornek.com"
        error={errors.email?.message}
        {...register('email')}
      />
      <Controller
        control={control}
        name="permissions"
        render={({ field }) => <PermissionList value={field.value} onChange={field.onChange} />}
      />

      <FormError error={inviteMutation.error ?? withdrawMutation.error} />
      <div className="mt-1 flex flex-wrap justify-end gap-3">
        {pending && (
          <button
            type="button"
            onClick={() => {
              confirmAction({
                title: 'Davet geri alınsın mı?',
                message: 'Gönderilen link artık açılmaz.',
                confirmLabel: 'Geri al',
                onConfirm: () => withdrawMutation.mutate(),
              })
            }}
            disabled={withdrawMutation.isPending}
            className={`${secondaryButton} mr-auto text-clay-dark`}
          >
            Daveti geri al
          </button>
        )}
        <button type="button" onClick={onClose} className={secondaryButton}>
          Vazgeç
        </button>
        <button type="submit" disabled={inviteMutation.isPending} className={primaryButton}>
          {inviteMutation.isPending ? 'Gönderiliyor…' : pending ? 'Yeniden gönder' : 'Davet gönder'}
        </button>
      </div>
    </form>
  )
}

// ---------------------------------------------------------------- hesap varken: yetki ve durum

function AccountForm({ account, onClose }: FormProps) {
  const queryClient = useQueryClient()
  const [permissions, setPermissions] = useState<Permission[]>(account.permissions)
  const active = account.status === 'Aktif'
  const userId = account.userId!

  const refresh = () => queryClient.invalidateQueries({ queryKey: staffAccountsQueryKey })

  const permissionsMutation = useMutation({
    mutationFn: () => updateAccountPermissions(userId, permissions),
    onSuccess: (message) => {
      toast.success(message)
      void refresh()
      onClose()
    },
    meta: { inlineError: true },
  })

  const statusMutation = useMutation({
    mutationFn: () => updateAccountStatus(userId, !active),
    onSuccess: (message) => {
      toast.success(message)
      void refresh()
      onClose()
    },
    meta: { inlineError: true },
  })

  const toggleStatus = () => {
    confirmAction({
      title: active ? 'Hesap pasif yapılsın mı?' : 'Hesap aktif yapılsın mı?',
      message: active
        ? `${account.staffFullName} panele giremeyecek, açık oturumları kapanacak.`
        : `${account.staffFullName} panele yeniden girebilecek.`,
      confirmLabel: active ? 'Pasif yap' : 'Aktif yap',
      onConfirm: () => statusMutation.mutate(),
    })
  }

  const changed =
    permissions.length !== account.permissions.length ||
    permissions.some((permission) => !account.permissions.includes(permission))

  return (
    <div className="flex flex-col gap-4">
      <div className="flex flex-wrap items-center justify-between gap-2 rounded-xl border border-line-soft bg-sand-soft px-3.5 py-3 text-sm">
        <span className="truncate">{account.email}</span>
        <span
          className={`rounded-full px-2.5 py-1 text-xs font-semibold ${accountBadgeClasses[account.status]}`}
        >
          {account.status}
        </span>
      </div>

      <PermissionList value={permissions} onChange={setPermissions} />

      <FormError error={permissionsMutation.error ?? statusMutation.error} />
      <div className="mt-1 flex flex-wrap justify-end gap-3">
        <button
          type="button"
          onClick={toggleStatus}
          // pasif personelin hesabı açılamaz; önce personel aktif yapılır
          disabled={statusMutation.isPending || (!active && !account.staffIsActive)}
          className={`${secondaryButton} mr-auto ${active ? 'text-clay-dark' : ''}`}
        >
          {active ? 'Hesabı pasif yap' : 'Hesabı aktif yap'}
        </button>
        <button type="button" onClick={onClose} className={secondaryButton}>
          Vazgeç
        </button>
        <button
          type="button"
          onClick={() => permissionsMutation.mutate()}
          disabled={!changed || permissionsMutation.isPending}
          className={primaryButton}
        >
          {permissionsMutation.isPending ? 'Kaydediliyor…' : 'Yetkileri kaydet'}
        </button>
      </div>
    </div>
  )
}
