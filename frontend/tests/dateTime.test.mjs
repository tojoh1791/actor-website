import assert from 'node:assert/strict'
import test from 'node:test'
import { bookingDate, fromOsloInput, osloDate, toOsloInput } from '../src/lib/dateTime.ts'

test('Oslo input preserves winter and summer time independently of the computer timezone', () => {
  for (const [local, utc] of [
    ['2027-01-15T18:30', '2027-01-15T17:30:00.000Z'],
    ['2027-06-18T18:30', '2027-06-18T16:30:00.000Z'],
  ]) {
    assert.equal(fromOsloInput(local), utc)
    assert.equal(toOsloInput(utc), local)
  }
})

test('daylight-saving gaps and repeated hours cannot silently change the event time', () => {
  assert.throws(() => fromOsloInput('2026-03-29T02:30'), /entydig klokkeslett/)
  assert.throws(() => fromOsloInput('2026-10-25T02:30'), /entydig klokkeslett/)
  assert.equal(fromOsloInput('2026-10-25T01:30'), '2026-10-24T23:30:00.000Z')
  assert.equal(fromOsloInput('2026-10-25T03:30'), '2026-10-25T02:30:00.000Z')
})

test('date-only bookings and the minimum booking date use the Oslo calendar day', () => {
  assert.equal(bookingDate(null), 'Dato ikke avklart')
  assert.equal(bookingDate('2027-06-18'), '18. juni 2027')
  assert.equal(osloDate(Date.parse('2027-06-17T22:30:00Z')), '2027-06-18')
  assert.equal(osloDate(Date.parse('2027-01-14T23:30:00Z')), '2027-01-15')
})
