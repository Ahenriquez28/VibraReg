import './Block_4.css'
import NcrImage from '../../public/Ncr_Image.png'
import bofalogo from '../../public/bofalogo.png'
import GeVernova from '../../public/GeVernova.png'
import CornerBuilding from '../../public/CornerBuilding.png'

function SponsorsInfo() {
    return (
        <div className="fourth-block">
            {/* Abstract background shapes */}
            <div className="sponsor-shapes">
                <div className="sponsor-shape shape-1"></div>
                <div className="sponsor-shape shape-2"></div>
            </div>

            <div className="sponsors-container">
                <div className="sponsors-header">
                    <h2 className="sponsors-title">OUR SPONSORS</h2>
                    <p className="sponsors-subtitle">
                        A round of applause to our sponsors, who make this all possible.
                    </p>
                </div>

                <div className="sponsors-grid">
                    <div className="sponsor-card">
                        <img src={NcrImage} alt="NCR Voyix" className="sponsor-logo"/>
                    </div>
                    <div className="sponsor-card">
                        <img src={bofalogo} alt="Bank of America" className="sponsor-logo"/>
                    </div>
                    <div className="sponsor-card">
                        <img src={GeVernova} alt="GE Vernova" className="sponsor-logo"/>
                    </div>
                </div>

                {/* Centered button wrapper */}
                <div className="sponsor-cta-wrapper">
                    <a 
                        href="/VibraAtl-Spons.pdf" 
                        target="_blank" 
                        rel="noopener noreferrer" 
                        className="sponsor-cta"
                    >
                        <span>Interested In Sponsoring?</span>
                        <svg className="arrow-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                            <line x1="5" y1="12" x2="19" y2="12" strokeWidth={2}/>
                            <polyline points="12 5 19 12 12 19" strokeWidth={2}/>
                        </svg>
                    </a>
                </div>
            </div>

            <img src={CornerBuilding} alt="Corner Building" className="corner-building"/>
        </div>
    )
}

export default SponsorsInfo