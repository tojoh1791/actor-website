import { useCallback, useEffect, useState } from 'react'
import type { SetStateAction } from 'react'
import { ApiError, api, errorMessage } from '../lib/http'

interface QuerySnapshot<T> {
  key: string
  data: T | null
  error: string
  errorStatus: number | null
}

// Shared GET lifecycle: cancel old requests and never show results for an old URL.
// Form submissions remain explicit event handlers in the components that own them.
export function useApiQuery<T>(path: string | null) {
  const [revision, setRevision] = useState(0)
  const key = `${path}\0${revision}`
  const [snapshot, setSnapshot] = useState<QuerySnapshot<T> | null>(null)

  useEffect(() => {
    if (!path) return
    const controller = new AbortController()

    api<T>(path, { signal: controller.signal })
      .then((data) => {
        if (!controller.signal.aborted) setSnapshot({ key, data, error: '', errorStatus: null })
      })
      .catch((reason) => {
        if (!controller.signal.aborted)
          setSnapshot({
            key,
            data: null,
            error: errorMessage(reason),
            errorStatus: reason instanceof ApiError ? reason.status : null,
          })
      })

    return () => controller.abort()
  }, [path, key])

  const reload = useCallback(() => setRevision((value) => value + 1), [])
  const setData = useCallback(
    (update: SetStateAction<T | null>) => {
      setSnapshot((previous) => ({
        key,
        data:
          typeof update === 'function'
            ? (update as (value: T | null) => T | null)(
                previous?.key === key ? previous.data : null,
              )
            : update,
        error: '',
        errorStatus: null,
      }))
    },
    [key],
  )

  // Derive loading from the request key instead of resetting state inside an effect.
  const current = snapshot?.key === key ? snapshot : null
  return {
    data: current?.data ?? null,
    loading: path !== null && current === null,
    error: current?.error ?? '',
    errorStatus: current?.errorStatus ?? null,
    reload,
    setData,
  }
}
