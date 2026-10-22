export interface ActorEvent {
  id: string
  title: string
  description: string
  startsAt: string
  endsAt: string | null
  location: string
  address: string
  organizer: string | null
  externalUrl: string | null
  imageUrl: string | null
}
