import './Block_3.css'

import DropPoint from '../../public/dropPoint.png'
import Agenda from '../../public/VibraAgenda.pdf'
import Cmi from '../../public/cmi.png'
import SideCity from '../../public/SideCity.png'

function LocationInfo() {
    return (
        <div className="third-block">
            {/* Graffiti-style spray paint splatters */}
            <div className="paint-splatters">
                <div className="splatter splatter-1"></div>
                <div className="splatter splatter-2"></div>
                <div className="splatter splatter-3"></div>
                <div className="splatter splatter-4"></div>
            </div>

            {/* Street art drip effect */}
            <div className="paint-drips">
                <div className="drip drip-1"></div>
                <div className="drip drip-2"></div>
                <div className="drip drip-3"></div>
            </div>

            {/* Halftone dots pattern (comic book style) */}
            <div className="halftone-pattern"></div>

            {/* City image with artistic treatment */}
            <img src={SideCity} alt="" className="side-city"/>
            <div className="city-overlay-effect"></div>

            {/* Location pin section with street art vibe */}
            <div className="location-pin-section">
                <div className="pin-container">
                    <img src={DropPoint} alt="Location Pin" className="drop-point"/>
                    <div className="address-tag">
                        <div className="tag-stripe"></div>
                        <span className="address-text">25 PARK PL NE, ATLANTA, GA 30303</span>
                    </div>
                </div>
            </div>

            {/* Main content with graffiti styling */}
            <div className="venue-content">
                <div className="venue-left">
                    {/* Graffiti-style title */}
                    <div className="venue-title-container">
                        <h2 className="venue-title">
                            <span className="title-line-1">OUR</span>
                            <span className="title-line-2">VENUE</span>
                        </h2>
                        {/* Spray paint outline effect */}
                        <div className="title-outline">VENUE</div>
                    </div>

                    {/* University name with stencil effect */}
                    <div className="university-name">
                        <div className="stencil-text">Georgia State University</div>
                        <div className="stencil-text">Creative Media Industries</div>
                        <div className="stencil-text">Institute</div>
                    </div>

                    {/* Call-to-action button with street art style */}
                    <a 
                        href={Agenda} 
                        target="_blank" 
                        rel="noopener noreferrer" 
                        className="agenda-button"
                    >
                        <span className="button-text">VIEW HACKATHON AGENDA</span>
                        <div className="button-shine"></div>
                        <div className="button-shadow"></div>
                    </a>
                </div>

                {/* Building image with artistic frame */}
                <div className="venue-right">
                    <div className="image-frame">
                        <div className="frame-corner tl"></div>
                        <div className="frame-corner tr"></div>
                        <div className="frame-corner bl"></div>
                        <div className="frame-corner br"></div>
                        <img src={Cmi} alt="CMI Building" className="cmi-image"/>
                        <div className="image-glitch"></div>
                    </div>
                    {/* Tape effect like street posters */}
                    <div className="tape-strip tape-top"></div>
                    <div className="tape-strip tape-bottom"></div>
                </div>
            </div>

            {/* Abstract geometric shapes */}
            <div className="abstract-shapes">
                <div className="shape shape-circle"></div>
                <div className="shape shape-triangle"></div>
                <div className="shape shape-square"></div>
            </div>
        </div>
    )
}

export default LocationInfo