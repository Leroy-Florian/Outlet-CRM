import { Effect } from "effect"

export class ApiError {
  readonly _tag = "ApiError"
  constructor(
    readonly status: number,
    readonly message: string,
  ) {}
}

export const fetchJson = <T>(path: string): Effect.Effect<T, ApiError> =>
  Effect.tryPromise({
    try: async () => {
      const response = await fetch(path)
      if (!response.ok) {
        throw new ApiError(response.status, `Request to ${path} failed`)
      }
      return (await response.json()) as T
    },
    catch: (error) =>
      error instanceof ApiError ? error : new ApiError(0, String(error)),
  })

export interface ProspectDto {
  readonly id: string
  readonly name: string
  readonly email: string
  readonly company: string | null
  readonly stage: string
  readonly createdAt: string
}

export interface DownloadTrendPointDto {
  readonly capturedAt: string
  readonly totalDownloads: number
  readonly delta: number
}

export interface PaymentDto {
  readonly id: string
  readonly amount: number
  readonly currency: string
  readonly source: string
  readonly externalReference: string
  readonly status: string
  readonly createdAt: string
}

export const listProspects = fetchJson<ReadonlyArray<ProspectDto>>("/api/prospects/")
export const listPayments = fetchJson<ReadonlyArray<PaymentDto>>("/api/payments/")
export const getDownloadTrend = (packageId: string) =>
  fetchJson<ReadonlyArray<DownloadTrendPointDto>>(
    `/api/analytics/packages/${encodeURIComponent(packageId)}/trend`,
  )
