export default function AdminRequestError({
  message,
  onRetry,
}: {
  message: string
  onRetry: () => void
}) {
  return (
    <div role="alert" className="admin-error">
      <p>{message}</p>
      <button className="admin-secondary" type="button" onClick={onRetry}>
        Prøv igjen
      </button>
    </div>
  )
}
