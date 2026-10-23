import { spawn } from 'node:child_process'
import { fileURLToPath } from 'node:url'

// The parent Node process loads the ignored .env before starting .NET.
const child = spawn('dotnet', process.argv.slice(2), {
  cwd: fileURLToPath(new URL('../', import.meta.url)),
  stdio: 'inherit',
  env: process.env,
})
child.on('error', (error) => {
  console.error(error.message)
  process.exitCode = 1
})
child.on('exit', (code) => {
  process.exitCode = code ?? 1
})
