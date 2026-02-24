import './Block_4.css'
import NcrImage from '../../public/Ncr_Image.png'
import CornerBuilding from '../../public/CornerBuilding.png'

function SponsorsInfo() {
    return (
        <div className="Fourth-Block">
            <div className="SponsorsTextImage">
                <div className="Sponsors-Text">A round of applause to our sponsors, who make this all possible.</div>
                <img src={NcrImage} alt="Ncr" className="NcrVoyix"/>
            </div>
            <a href="/VibraAtl-Spons.pdf" target="_blank" rel="noopener noreferrer" className="Sponsor-link"> Interested In Sponsoring</a>
            <img src={CornerBuilding} alt="Corner" className="CornerBuilding"/>

        </div>
    )
}

export default SponsorsInfo
