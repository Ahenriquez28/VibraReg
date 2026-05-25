import { useEffect, useRef } from 'react'
import './Block_2.css'

import Confetti from '../../public/Confetti.png'

function ReasonsForJoin() {
    const sectionRef = useRef<HTMLDivElement>(null)

    // Fade in on scroll
    useEffect(() => {
        const observer = new IntersectionObserver(
            (entries) => {
                entries.forEach((entry) => {
                    if (entry.isIntersecting) {
                        entry.target.classList.add('visible')
                    }
                })
            },
            { threshold: 0.2 }
        )

        if (sectionRef.current) {
            observer.observe(sectionRef.current)
        }

        return () => observer.disconnect()
    }, [])

    return (
        <div className="second-block" ref={sectionRef}>
            {/* Gradient background orbs */}
            <div className="gradient-bg-belong">
                <div className="gradient-orb-belong orb-belong-1"></div>
                <div className="gradient-orb-belong orb-belong-2"></div>
                <div className="gradient-orb-belong orb-belong-3"></div>
            </div>

            <img src={Confetti} alt="Confetti" className="confetti"/>
            
            <div className="overlay">
                <div className="belong-title">
                    <span className="text-belong">YOU BELONG HERE</span>
                </div>
                
                <div className="reasons-belong">
                    <div className="reasons-text">
                        <div className="reason-card card-1">
                            <div className="card-glow"></div>
                            <div className="card-icon">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                                    <polyline points="16 18 22 12 16 6"></polyline>
                                    <polyline points="8 6 2 12 8 18"></polyline>
                                </svg>
                            </div>
                            <h3 className="card-title">Build Your Skills</h3>
                            <p>
                                Whether you've barely ever coded in your life or 
                                have participated in twenty hackathons, Vibra ATL is
                                here to give you a platform to build, to learn, and 
                                to create.
                            </p>
                        </div>

                        <div className="reason-card card-2">
                            <div className="card-glow"></div>
                            <div className="card-icon">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                                </svg>
                            </div>
                            <h3 className="card-title">Inclusive Community</h3>
                            <p>
                                As Georgia's largest SHPE hackathon, we recognize
                                that tech spaces need to do more to create environments 
                                where hackers of marginalized backgrounds can thrive.
                            </p>
                        </div>

                        <div className="reason-card card-3">
                            <div className="card-glow"></div>
                            <div className="card-icon">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
                                </svg>
                            </div>
                            <h3 className="card-title">Change the World</h3>
                            <p>
                                As long as you're passionate about technology and looking
                                to change the world, you belong at Vibra ATL.
                            </p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    )
}

export default ReasonsForJoin