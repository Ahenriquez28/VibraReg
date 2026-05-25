import { useEffect, useRef } from 'react'
import './InitalView.css'
import { useNavigate } from 'react-router-dom'

import VibraLogo from '../../public/VIbra_Logo.png'
import CityView from '../../public/City_View.png'
import FireWorks from '../../public/Fire.png'

function InitalView() {
    const navigate = useNavigate()
    const logoRef = useRef<HTMLImageElement>(null)

    // Mouse tracking for logo tilt effect
    useEffect(() => {
        const handleMouseMove = (e: MouseEvent) => {
            if (!logoRef.current) return
            
            const rect = logoRef.current.getBoundingClientRect()
            const centerX = rect.left + rect.width / 2
            const centerY = rect.top + rect.height / 2
            
            const deltaX = (e.clientX - centerX) / rect.width
            const deltaY = (e.clientY - centerY) / rect.height
            
            const rotateX = deltaY * -10 // Tilt up/down
            const rotateY = deltaX * 10  // Tilt left/right
            
            logoRef.current.style.transform = `
                translateX(60%) 
                perspective(1000px) 
                rotateX(${rotateX}deg) 
                rotateY(${rotateY}deg)
            `
        }
        
        const resetLogo = () => {
            if (logoRef.current) {
                logoRef.current.style.transform = 'translateX(60%) perspective(1000px) rotateX(0deg) rotateY(0deg)'
            }
        }
        
        window.addEventListener('mousemove', handleMouseMove)
        window.addEventListener('mouseleave', resetLogo)
        
        return () => {
            window.removeEventListener('mousemove', handleMouseMove)
            window.removeEventListener('mouseleave', resetLogo)
        }
    }, [])

    return (
        <div className="inital-view">
            {/* Animated background gradients */}
            <div className="gradient-bg">
                <div className="gradient-orb orb-1"></div>
                <div className="gradient-orb orb-2"></div>
                <div className="gradient-orb orb-3"></div>
            </div>

            {/* Floating particles */}
            <div className="particles">
                {[...Array(20)].map((_, i) => (
                    <div key={i} className="particle" style={{
                        left: `${Math.random() * 100}%`,
                        animationDelay: `${Math.random() * 5}s`,
                        animationDuration: `${5 + Math.random() * 10}s`
                    }}></div>
                ))}
            </div>

            <div className="logo-event-row">
                <img 
                    ref={logoRef}
                    src={VibraLogo} 
                    alt="Vibra Logo" 
                    className="vibra-logo"
                />

                <div className="event-info">
                    <div className="event-details">
                        <span className="pill animate-slide-in" style={{ animationDelay: '0.1s' }}>
                            <svg className="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                            </svg>
                            Atlanta, Georgia
                        </span>
                        <span className="pill animate-slide-in" style={{ animationDelay: '0.2s' }}>
                            <svg className="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                                <rect x="3" y="4" width="18" height="18" rx="2" ry="2" strokeWidth={2}/>
                                <line x1="16" y1="2" x2="16" y2="6" strokeWidth={2}/>
                                <line x1="8" y1="2" x2="8" y2="6" strokeWidth={2}/>
                                <line x1="3" y1="10" x2="21" y2="10" strokeWidth={2}/>
                            </svg>
                            April 4th – 5th
                        </span>
                        <span className="pill animate-slide-in" style={{ animationDelay: '0.3s' }}>
                            <svg className="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                                <circle cx="12" cy="12" r="10" strokeWidth={2}/>
                                <polyline points="12 6 12 12 16 14" strokeWidth={2}/>
                            </svg>
                            24 hours
                        </span>
                    </div>

                    <div className="event-tagline-container glass-card animate-fade-in">
                        <div className="glow-effect"></div>
                        
                        <div className="event-tagline">
                            <span className="text-shimmer">BECOME A HACKER</span>
                        </div>
                        
                        <p className="event-subtext">
                            Georgia’s largest SHPE hackathon unites student innovators from across the state to build, learn, and grow together. We’re creating an environment where diverse talent is empowered, collaboration drives innovation, and the next generation of tech leaders takes shape.

                        </p>
                        
                        <button 
                            onClick={() => {
                                console.log('Button clicked!')
                                navigate('/register')
                            }} 
                            className="register-button"
                        >
                            <span className="button-text">Register Now</span>
                            <svg className="arrow-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                                <line x1="5" y1="12" x2="19" y2="12" strokeWidth={2}/>
                                <polyline points="12 5 19 12 12 19" strokeWidth={2}/>
                            </svg>
                            <div className="button-glow"></div>
                        </button>

                        <div className="stats-row">
                            <div className="stat-item">
                                <div className="stat-number">100+</div>
                                <div className="stat-label">Hackers</div>
                            </div>
                            <div className="stat-divider"></div>
                            <div className="stat-item">
                                <div className="stat-number">Impact & Growth</div>
                                <div className="stat-label">Build. Learn. Launch </div>
                            </div>
                            <div className="stat-divider"></div>
                            <div className="stat-item">
                                <div className="stat-number">24/7</div>
                                <div className="stat-label">Support</div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            
            <div className="city-view-container">
                {/* Fog overlay */}
                <div className="fog-overlay">
                    <div className="fog fog-1"></div>
                    <div className="fog fog-2"></div>
                    <div className="fog fog-3"></div>
                </div>
                
                <img src={CityView} alt="City View" className="city-view-image"/>
            </div>
            
            <img src={FireWorks} alt="FireWorks" className="fireworks animate-pulse"/>
        </div>
    )
}

export default InitalView