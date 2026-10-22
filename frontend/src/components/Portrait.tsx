import { site } from '../content/site'

export default function Portrait({ priority = false }: { priority?: boolean }) {
  return <figure className="portrait">
    <img src={site.portrait} alt="Portrett av Arben Bala i svart genser mot en mørk bakgrunn" width="750" height="1125" loading={priority ? 'eager' : 'lazy'} fetchPriority={priority ? 'high' : 'auto'} />
    <figcaption>Foto: {site.photoCredit}</figcaption>
  </figure>
}
