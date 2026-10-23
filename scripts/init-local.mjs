import { randomBytes } from 'node:crypto'
import { existsSync, writeFileSync } from 'node:fs'
import { fileURLToPath } from 'node:url'

const target = fileURLToPath(new URL('../.env', import.meta.url))
if (existsSync(target)) {
  console.log('.env finnes allerede og ble beholdt.')
} else {
  const password = randomBytes(32).toString('base64url')
  writeFileSync(
    target,
    [
      'POSTGRES_DB=actor_website',
      'POSTGRES_USER=actor_dev',
      'POSTGRES_PASSWORD=' + password,
      'ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=actor_website;Username=actor_dev;Password=' +
        password +
        '"',
      'ASPNETCORE_ENVIRONMENT=Development',
      '',
    ].join('\n'),
    { flag: 'wx', mode: 0o600 },
  )
  console.log('Opprettet lokal .env med et tilfeldig databasepassord.')
}
