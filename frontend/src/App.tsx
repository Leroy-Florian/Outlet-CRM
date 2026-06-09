import { Effect } from "effect"
import { useEffect, useState } from "react"
import {
  listPayments,
  listProspects,
  type PaymentDto,
  type ProspectDto,
} from "./api/client"

const useApi = <T,>(effect: Effect.Effect<T, unknown>, initial: T): T => {
  const [value, setValue] = useState(initial)

  useEffect(() => {
    void Effect.runPromise(
      effect.pipe(Effect.catchAll(() => Effect.succeed(initial))),
    ).then(setValue)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  return value
}

export const App = () => {
  const prospects = useApi<ReadonlyArray<ProspectDto>>(listProspects, [])
  const payments = useApi<ReadonlyArray<PaymentDto>>(listPayments, [])

  return (
    <main style={{ fontFamily: "system-ui", margin: "2rem auto", maxWidth: 960 }}>
      <h1>Outlet CRM</h1>

      <section>
        <h2>Prospects ({prospects.length})</h2>
        <ul>
          {prospects.map((p) => (
            <li key={p.id}>
              {p.name} — {p.email} — <strong>{p.stage}</strong>
            </li>
          ))}
        </ul>
      </section>

      <section>
        <h2>Paiements ({payments.length})</h2>
        <ul>
          {payments.map((p) => (
            <li key={p.id}>
              {p.amount} {p.currency} via {p.source} — <strong>{p.status}</strong>
            </li>
          ))}
        </ul>
      </section>
    </main>
  )
}
