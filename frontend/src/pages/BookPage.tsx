import { Link } from 'react-router-dom'
import { site } from '../content/site'

export default function BookPage() {
  return <>
    <section className="book-page">
      <figure className="book-image"><img src={site.book.cover} alt="Balkanbrødre av Anders Moe. Rødt omslag med svart tohodet ørn." width="446" height="700" fetchPriority="high" /><figcaption>Pullman Publishing · 2025</figcaption></figure>
      <div><p className="eyebrow">Roman av Anders Moe</p><h1>Balkanbrødre</h1><p className="lead">Basert på livet til Arben Bala.</p><p>En melding om en kidnapping sender Beni fra Oslo mot Kosovo. Reisen vekker minner fra oppveksten i Mitrovica, og en fortid han ikke har lagt bak seg.</p><p><i>Balkanbrødre</i> er en thriller med utgangspunkt i Arbens liv. Det er en roman, der en personlig historie har blitt bearbeidet til skjønnlitteratur.</p><dl className="book-facts"><div><dt>Forfatter</dt><dd>{site.book.author}</dd></div><div><dt>Forlag</dt><dd>{site.book.publisher}</dd></div><div><dt>Utgitt</dt><dd>{site.book.year}</dd></div></dl><a className="button" href={site.book.informationUrl} target="_blank" rel="noreferrer">Bokinformasjon hos ARK <span aria-hidden="true">↗</span></a></div>
    </section>
    <section className="reading-note section-space"><p className="eyebrow">Fra bok til møte</p><h2>Hør Arben fortelle.</h2><p>Vil du møte personen bak historien? Bli kjent med foredraget «Uten frykt», eller se kommende offentlige arrangementer.</p><div className="actions"><Link className="text-link" to="/talks">Om foredraget <span aria-hidden="true">→</span></Link><Link className="text-link" to="/events">Se arrangementer <span aria-hidden="true">→</span></Link></div></section>
  </>
}
