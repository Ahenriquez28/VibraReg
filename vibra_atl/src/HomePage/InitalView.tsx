import './InitalView.css'
import { useNavigate } from 'react-router-dom';

import VibraLogo from '../../public/VIbra_Logo.png'
import CityView from '../../public/City_View.png'
import VibraText from '../../public/Vibra_Text.png'
import HackathonText from '../../public/Hackathon_Text.png'
import FireWorks from '../../public/Fire.png'

function InitalView() {
    const navigate = useNavigate();

    return (
        <div className="inital-view">
            <div className="logo-event-row">
                <img src={VibraLogo} alt="Vibra Logo" className="vibra-logo"/>

                <div className="event-info">
                    <div className="event-details">
                    <span>Atlanta, Georgia</span>
                    <span>April 4th – 5th</span>
                    <span>30 hours</span>
                    </div>

                    <div className="event-tagline-container">
                        <div className="event-tagline">
                        BECOME A HACKER
                        </div>
                        <p className="event-subtext">
                        Join Vibra ATL Hackathon Join Vibra ATL HackathonJoin Vibra ATL HackathonJoin Vibra ATL 
                        HackathonJoin Vibra ATL HackathonJoin Vibra ATL HackathonvvvJoin Vibra ATL HackathonJoi
                        n Vibra ATL HackathonJoin Vibra ATL HackathonJoin Vibra ATL HackathonJoin Vibra ATL Hack
                        athonvJoin Vibra ATL HackathonJoin Vibra ATL HackathonJoin Vibra ATL HackathonJoin Vibra 
                        ATL HackathonJoin Vibra ATL HackathonJoin Vibra ATL Hackathon
                        </p>
                        <button 
                            onClick={() => {
                                console.log('Button clicked!');
                                navigate('/register');
                            }} 
                            className="register-button"
                        >
                            Register
                        </button>
                    </div>
                </div>
            </div>
            
            <div className = "city-view-container">
                <img src={CityView} alt="City View" className="city-view-image"/>

                <div className="overlay-content">
                    <img src={VibraText} alt="Vibra Text" className="vibra-text"/>
                    <img src={HackathonText} alt="Hackathon Text" className="hackathon-text" />
                </div>

            </div>
            <img src={FireWorks} alt="FireWorks" className="FireWorks"/>
        </div>
    )
}

export default InitalView
