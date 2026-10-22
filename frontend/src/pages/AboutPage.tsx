import { Link } from 'react-router-dom'
import Portrait from '../components/Portrait'
import { selectedCredits, site } from '../content/site'

export default function AboutPage() {
  return <>
    <section className="page-split">
      <div><p className="eyebrow">Mennesket bak rollene</p><h1>Om Arben.</h1><p className="lead">Skuespiller, foredragsholder og et liv som har blitt til litteratur.</p><p>Arben Bala har bakgrunn fra Kosovo og en skuespillerkarriere i Norge. Han medvirket i Joachim Triers <i>Reprise</i> i 2006, og har siden hatt roller i en rekke norske filmer og TV-serier.</p><p>Blant rollene er Khabib i <i>Jordbrukerne</i>, Eron i <i>Pørni</i> og Marcus i <i>Krypto Kings</i>. Han har også medvirket i blant annet <i>Lilyhammer</i>, <i>Okkupert</i> og <i>Nede</i>.</p><p>I dag møter han også publikum som foredragsholder med «Uten frykt».</p><Link className="text-link" to="/talks">Les om foredraget <span aria-hidden="true">→</span></Link></div>
      <Portrait priority />
    </section>
    <section className="section-space" aria-labelledby="credits-title"><div className="section-heading"><div><p className="eyebrow">Film og TV</p><h2 id="credits-title">Utvalgte roller</h2></div><a className="text-link" href={site.managementProfile} target="_blank" rel="noreferrer">Full filmografi hos Pullman <span aria-hidden="true">↗</span></a></div><div className="filmography">{selectedCredits.map(credit => <article key={credit.title}><span className="year">{credit.year}</span><h3>{credit.title}</h3><p>{credit.role}</p></article>)}</div></section>
    <section className="reading-note section-space"><p className="eyebrow">Fra liv til roman</p><h2>Historien bak Balkanbrødre</h2><p>Arbens liv er utgangspunktet for Anders Moes roman <i>Balkanbrødre</i>. Boken er skjønnlitteratur basert på livshistorien hans.</p><Link className="text-link" to="/book">Utforsk boken <span aria-hidden="true">→</span></Link></section>
  </>
}
