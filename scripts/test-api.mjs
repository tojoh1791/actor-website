import { spawn } from 'node:child_process'

// Each test creates and removes its own uniquely named PostgreSQL database.
const connection = process.env.ACTOR_TEST_POSTGRES ?? process.env.ConnectionStrings__DefaultConnection
if (!connection) throw new Error('Sett ACTOR_TEST_POSTGRES eller kjør testen med lokal .env.')
const child = spawn('dotnet', ['test', 'ActorWebsite.slnx', '--configuration', 'Release', '--disable-build-servers', '-m:1', ...process.argv.slice(2)], {
  stdio: 'inherit',
  env: { ...process.env, ACTOR_TEST_POSTGRES: connection },
})
child.on('error', error => { console.error(error.message); process.exitCode = 1 })
child.on('exit', code => { process.exitCode = code ?? 1 })
