import './Block_2.css'

import Confetti from '../../public/Confetti.png'


function ReasonsForJoin() {
    return (
        <div className="Second-block">
            <img src={Confetti} alt="Vibra Text" className="confetti"/>
            <div className="overlay">
                <div className = "Belong">You Belong Here</div>
                <div className ="Reasons-belong">
                    <div className="Reasons-Text">
                        <span>
                            Whether you’ve barely ever coded in your life or 
                            have participated in twenty hackathons, Vibra ATL is
                            here to give you a platform to build, to learn, and 
                            to create.
                        </span>
                        <span>
                            As Georgia's largest SHPE hackathon, we recognize
                            that tech spaces need to do more to create environments 
                            where hackers of marginalized backgrounds can thrive.
                        </span>
                        <span>
                            As long as you’re passionate about technology and looking
                            to change the world, you belong at Vibra ATL.
                        </span>
                    </div>
                </div>
            </div>
            
        </div>

    )
}

export default ReasonsForJoin
