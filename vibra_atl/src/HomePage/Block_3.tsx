import './Block_3.css'

import DropPoint from '../../public/dropPoint.png'
import SponsorPacket from '../../public/VibraAtl-Spons.pdf'
import Cmi from '../../public/cmi.png'
import SideCity from '../../public/SideCity.png'

function LocationInfo() {
    return (
        <div className="Third-block">
            <div className="Location">
                <img src={DropPoint} alt="Vibra Text" className="dropPoint"/>
                <div className="Address">25 Park Pl NE, Atlanta, GA 30303</div>
            </div>
            <div className="Location_View">
                <div className="Address-Text">
                    <span>
                        Our Venue
                    </span>
                    <span>
                        Georgia State University Creative Media Industries Institute
                    </span>
                    <a href={SponsorPacket} target="_blank" rel="noopener noreferrer" className="pdf-link"
                    > View Hackathon Agenda</a>
                </div>
                <img src={Cmi} alt="Vibra Text" className="cmi"/>
            </div>
            <img src={SideCity} alt="" className="SideCity"/>
        </div>
    )
}

export default LocationInfo
