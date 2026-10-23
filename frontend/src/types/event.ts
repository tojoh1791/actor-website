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

export interface AdminEvent extends ActorEvent {
  isPublished: boolean
  createdAt: string
  updatedAt: string
}
export type EventInput = Pick<
  ActorEvent,
  | 'title'
  | 'description'
  | 'startsAt'
  | 'endsAt'
  | 'location'
  | 'address'
  | 'organizer'
  | 'externalUrl'
  | 'imageUrl'
>
