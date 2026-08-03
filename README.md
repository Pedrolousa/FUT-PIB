# FUT PIB

Sistema web para cadastro de jogadores, avaliações anônimas e sorteio equilibrado de times.

## Execução local

Por padrão, usa SQL Server Express:

```text
Server=.\SQLEXPRESS;Database=FutPibDb;Trusted_Connection=True;TrustServerCertificate=True
```

Login local inicial:

```text
Usuário: admin
Senha: FutPib@2026
```

## Publicação

O projeto está pronto para Docker + Render + PostgreSQL/Supabase.

Variáveis obrigatórias em produção:

- `DATABASE_URL`
- `ADMIN_PASSWORD`
- `GROUP_CODE`

Consulte `PUBLICAR-GRATIS.txt`.
