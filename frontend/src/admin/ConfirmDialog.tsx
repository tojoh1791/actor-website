import { useEffect, useRef } from 'react'

export default function ConfirmDialog({
  title,
  description,
  confirmLabel,
  busy,
  onConfirm,
  onClose,
  error,
}: {
  title: string
  description: string
  confirmLabel: string
  busy: boolean
  onConfirm: () => void
  onClose: () => void
  error?: string
}) {
  const ref = useRef<HTMLDialogElement>(null)
  useEffect(() => {
    const dialog = ref.current
    dialog?.showModal()
    return () => {
      if (dialog?.open) dialog.close()
    }
  }, [])
  return (
    <dialog
      ref={ref}
      className="admin-dialog"
      aria-labelledby="confirm-title"
      aria-describedby="confirm-description"
      onCancel={(event) => {
        event.preventDefault()
        if (!busy) onClose()
      }}
    >
      <h2 id="confirm-title">{title}</h2>
      <p id="confirm-description">{description}</p>
      {error && (
        <p role="alert" className="admin-error">
          {error}
        </p>
      )}
      <div className="admin-stack">
        <button
          type="button"
          className="admin-secondary"
          disabled={busy}
          onClick={onClose}
          autoFocus
        >
          Avbryt
        </button>
        <button type="button" className="admin-danger" disabled={busy} onClick={onConfirm}>
          {busy ? 'Arbeider …' : confirmLabel}
        </button>
      </div>
    </dialog>
  )
}
