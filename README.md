# Outlet CRM

CRM / dashboard du projet [Outlet](https://github.com/Leroy-Florian/Outlet-CLI) :

- **Prospects** — pipeline New → Contacted → Qualified → Won/Lost, interactions.
- **Analytics** — snapshots des téléchargements NuGet (API azuresearch) et tendances (deltas).
- **Métriques API** — échantillons de latence/statut, statistiques par endpoint (p95, taux d'erreur).
- **Paiements** — modèle générique provider-agnostique (Pending → Settled/Failed → Refunded).

## Démarrage

```bash
# Backend (.NET 10, PostgreSQL requis)
dotnet build Crm.slnx -c Release
dotnet test Crm.slnx --filter "Category!=Live"
dotnet run --project src/Crm.Api

# Frontend (React + Vite + Effect)
cd frontend && npm install && npm run dev
```

L'authentification est déléguée à Outlet-SSO (JWT bearer, `Sso:Authority`).

Voir [CLAUDE.md](CLAUDE.md) pour l'architecture et les conventions.
