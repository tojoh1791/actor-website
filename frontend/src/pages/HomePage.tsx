import { Link } from 'react-router-dom'
import BookingLink from '../components/BookingLink'
import Portrait from '../components/Portrait'
import { selectedCredits, site } from '../content/site'

export default function HomePage() {
  return <>
    <section className="hero" aria-labelledby="home-title">
      <div className="hero-copy">
        <p className="eyebrow"><span className="small-line" /> Skuespiller og foredragsholder</p>
        <h1 id="home-title">Arben<br /><em>Bala.</em></h1>
        <p className="hero-intro">Fra lerretet til møtet med publikum.</p>
        <p>Skuespiller kjent fra <i>Jordbrukerne</i> og <i>Pørni</i>. Foredrag om tilhørighet, valg og veier videre.</p>
        <div className="actions"><Link className="button" to="/events">Se arrangementer <span aria-hidden="true">↗</span></Link><BookingLink className="text-link" /></div>
      </div>
      <Portrait priority />
    </section>

    <section className="credits-strip" aria-label="Utvalgte produksjoner"><span className="eyebrow">Kjent fra</span><div>{selectedCredits.map(credit => <span key={credit.title}>{credit.title}</span>)}</div></section>

    <section className="talk-feature section-space" aria-labelledby="talk-title">
      <div><p className="eyebrow">Foredraget</p><h2 id="talk-title">Uten frykt.<br /><em>Med håp.</em></h2></div>
      <div className="feature-copy"><p className="lead">En personlig fortelling om utenforskap, vanskelige valg og muligheten til å begynne på nytt.</p><p>I «Uten frykt» tar Arben utgangspunkt i sin egen livshistorie. Foredraget åpner for refleksjon rundt ansvar og tilhørighet.</p><Link className="text-link" to="/talks">Bli kjent med foredraget <span aria-hidden="true">→</span></Link></div>
    </section>

    <section className="book-feature section-space" aria-labelledby="book-title">
      <div className="book-image"><img src={site.book.cover} alt="Det røde bokomslaget til Balkanbrødre av Anders Moe" width="446" height="700" loading="lazy" /></div>
      <div><p className="eyebrow">Boken bak fortellingen</p><h2 id="book-title">Balkanbrødre</h2><p className="lead">Basert på livet til Arben Bala.</p><p>Anders Moes roman følger Beni på en reise mellom Norge og Kosovo, der fortiden griper inn i nåtiden.</p><p className="book-meta">Anders Moe · Pullman Publishing · 2025</p><Link className="text-link" to="/book">Les om boken <span aria-hidden="true">→</span></Link></div>
    </section>

    <section className="invitation section-space"><div><p className="eyebrow">For publikum og arrangører</p><h2>Et møte med Arben.</h2><p>Finn et offentlig arrangement, eller les om foredrag og booking for ditt eget arrangement.</p></div><div className="actions"><Link className="button" to="/events">Finn arrangementer <span aria-hidden="true">↗</span></Link><BookingLink className="text-link" /></div></section>
  </>
}
