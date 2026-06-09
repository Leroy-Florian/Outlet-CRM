# Outlet CRM

CRM / dashboard **multi-produits** (Outlet, FluxPDF, Accordent…). Le module
`Products` est la racine : chaque produit suit N packages (NuGet **et** npm)
et N repositories GitHub (issues/stars/forks) ; prospects, paiements,
snapshots de téléchargements et métriques d'API sont tous rattachés à un
`ProductId`. Le module `Organizations` porte les clients : prospects et
paiements peuvent être rattachés à une `OrganizationId` (optionnel —
sponsoring anonyme possible), une organisation pouvant payer pour plusieurs
produits. Sémantique des compteurs : NuGet = cumul total, npm = volume
glissant 30 jours.

## Commandes clés

- `dotnet build Crm.slnx -c Release` — **0 warning exigé** (TreatWarningsAsErrors).
- `dotnet test Crm.slnx --filter "Category!=Live"` — suite complète hors tests Live.
- `cd frontend && npm run build && npm test` — frontend.
- Mutation : `dotnet stryker` (config dans `stryker-config.json`, break à 80).

## Architecture (hexagonale + DDD strict)

```
Crm.Kernel.Shared        → building blocks (Result, Error, AggregateRoot, IClock) — dépend de rien
Crm.Core.Domain          → agrégats par module : Products, Prospects, Analytics, ApiMetrics, Payments
Crm.Core.Application     → ports (Abstractions/) + use cases retournant Result
Crm.Core.Infrastructure  → EF Core/Npgsql, HTTP NuGet, horloge — seul endroit pour l'IO
Crm.Api                  → composition root mince (minimal APIs)
frontend/                → React + Vite + Effect, TypeScript strict
```

Les tests d'architecture (`tests/Crm.Architecture.Tests`) verrouillent ces règles :
ne jamais les contourner, adapter le code.

## Conventions verrouillées

- .NET 10 / C# 14, CPM (`Directory.Packages.props`).
- Primary constructors et collection expressions obligatoires (erreurs de build).
- Pas de framework de mock : fakes maison dans `tests/**/Fakes/`.
- Nommage des tests : `Should_<Effet>_When_<Condition>`.
- Couverture ≥ 90 % Domain+Application, mutation Stryker ≥ 80.
- Les factories du Domain retournent `Result<T>`, jamais d'exception métier.
- Use cases sealed, un fichier = commande + use case.

## Authentification

Aucune identité gérée ici : l'API valide des JWT émis par **Outlet-SSO**
(config `Sso:Authority` / `Sso:Audience`). Sans authority configurée (dev
local), les endpoints sont ouverts.

## Base de données

PostgreSQL via EF Core (`ConnectionStrings:CrmDatabase`). Les migrations EF
restent à générer (`dotnet ef migrations add Initial`).
