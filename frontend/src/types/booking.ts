export const bookingTypes: Record<string, string> = {
  talk: 'Foredrag', book: 'Boksamtale eller litteraturarrangement', acting: 'Skuespilleroppdrag', other: 'Annet samarbeid',
}
export const bookingStatuses: Record<string, string> = {
  new: 'Ny', inProgress: 'Under behandling', agreed: 'Avtalt', declined: 'Avslått',
}
export interface BookingSummary {
  id: string; contactName: string; organization: string | null; eventType: string
  preferredDate: string | null; location: string; status: string; createdAt: string
}
export interface BookingDetails extends BookingSummary {
  email: string; phone: string | null; participants: number | null; message: string; updatedAt: string
}
export function bookingDate(value: string | null) {
  return value ? new Intl.DateTimeFormat('nb-NO', { dateStyle: 'long', timeZone: 'Europe/Oslo' })
    .format(new Date(value + 'T12:00:00Z')) : 'Dato ikke avklart'
}
