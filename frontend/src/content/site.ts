export interface ContactSettings {
  email: string | null
  phone: string | null
  bookingUrl: string | null
}

// Fill in only the channels agreed with Arben. null keeps them hidden in the UI.
// bookingUrl supports an external booking service or a future local form route.
export const contactSettings: ContactSettings = {
  email: null,
  phone: null,
  bookingUrl: null,
}

export const site = {
  name: 'Arben Bala',
  portrait: '/images/arben-bala.jpg',
  photoCredit: 'Are Næsset',
  managementProfile: 'https://pullman.as/talent/arben-bala',
  talk: {
    title: 'Uten frykt',
    themes: ['Utenforskap', 'Tilhørighet', 'Valg og ansvar', 'Håp og nye muligheter'],
    source: 'https://www.athenas.no/foredragsholdere/arben-bala/',
  },
  book: {
    title: 'Balkanbrødre',
    subtitle: 'Basert på livet til Arben Bala',
    author: 'Anders Moe',
    publisher: 'Pullman Publishing',
    year: 2025,
    cover: '/images/balkanbrodre.jpg',
    informationUrl: 'https://www.ark.no/produkt/balkanbrodre-9788293891291',
  },
}

export const selectedCredits = [
  { title: 'Pørni', role: 'Eron', year: '2022–2025' },
  { title: 'Krypto Kings', role: 'Marcus', year: '2023' },
  { title: 'Jordbrukerne', role: 'Khabib', year: '2021' },
  { title: 'Reprise', role: 'Sveins kamerat', year: '2006' },
]
