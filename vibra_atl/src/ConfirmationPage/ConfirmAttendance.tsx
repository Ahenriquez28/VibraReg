import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import './ConfirmAttendance.css';

const API_URL = import.meta.env.VITE_API_URL;

function ConfirmAttendance() {
  const { token } = useParams<{ token: string }>();
  const [status, setStatus] = useState<'loading' | 'success' | 'error' | 'already-confirmed'>('loading');
  const [message, setMessage] = useState('');
  const [studentName, setStudentName] = useState('');

  useEffect(() => {
    if (!token) {
      setStatus('error');
      setMessage('Invalid confirmation link');
      return;
    }

    // Automatically confirm when page loads
    fetch(`${API_URL}/confirm/${token}`)
      .then(res => res.json())
      .then(data => {
        if (data.success) {
          if (data.message.includes('already confirmed')) {
            setStatus('already-confirmed');
            setMessage('You have already confirmed your attendance!');
          } else {
            setStatus('success');
            setMessage(data.message);
          }
        } else {
          setStatus('error');
          setMessage(data.message || 'Failed to confirm attendance');
        }
      })
      .catch(err => {
        console.error('Confirmation error:', err);
        setStatus('error');
        setMessage('Failed to connect to server. Please try again later.');
      });
  }, [token]);

  return (
    <div className="confirm-container">
      <div className="confirm-card">
        {status === 'loading' && (
          <div className="loading-state">
            <div className="spinner"></div>
            <h2>Confirming your attendance...</h2>
            <p>Please wait a moment</p>
          </div>
        )}

        {status === 'success' && (
          <div className="success-state">
            <div className="success-icon">✅</div>
            <h1>Thank You for Confirming!</h1>
            <p className="subtitle">You're all set for Vibra ATL Hackathon</p>
            
            <div className="info-box">
              <h3>📅 Event Details:</h3>
              <ul>
                <li><strong>Date:</strong> April 5th, 2026</li>
                <li><strong>Time:</strong> 9:00 AM - 6:00 PM</li>
                <li><strong>Location:</strong> Georgia State University</li>
              </ul>
            </div>

            <div className="important-notice">
              <h3>⚠️ Important Notice:</h3>
              <p>
                <strong>Do NOT share this confirmation link!</strong><br />
                This link is unique to you and only works for your registration.
              </p>
              <p>
                If you have teammates, please ask them to check their own email 
                and confirm using their personal link.
              </p>
            </div>

            <div className="next-steps">
              <h3>What's Next?</h3>
              <p>✉️ You'll receive event updates via email</p>
              <p>📧 Check your inbox closer to the event date for more details</p>
              <p>🚀 Start thinking about what you want to build!</p>
            </div>

            <a href="/#/" className="back-home-btn">
              Back to Home
            </a>
          </div>
        )}

        {status === 'already-confirmed' && (
          <div className="already-confirmed-state">
            <div className="info-icon">ℹ️</div>
            <h1>Already Confirmed</h1>
            <p className="subtitle">{message}</p>
            
            <div className="info-box">
              <p>You previously confirmed your attendance. See you at the event!</p>
              <h3>📅 Event Details:</h3>
              <ul>
                <li><strong>Date:</strong> April 5th, 2026</li>
                <li><strong>Time:</strong> 9:00 AM - 6:00 PM</li>
                <li><strong>Location:</strong> Georgia State University</li>
              </ul>
            </div>

            <a href="/#/" className="back-home-btn">
              Back to Home
            </a>
          </div>
        )}

        {status === 'error' && (
          <div className="error-state">
            <div className="error-icon">❌</div>
            <h1>Oops! Something Went Wrong</h1>
            <p className="error-message">{message}</p>
            
            <div className="error-help">
              <h3>What can you do?</h3>
              <ul>
                <li>Check if the confirmation link is complete</li>
                <li>Make sure you haven't already confirmed</li>
                <li>Verify the deadline hasn't passed (April 1st, 5:00 PM)</li>
              </ul>
              <p>
                <strong>Need help?</strong><br />
                Contact us at <a href="mailto:shpe.gastate@gmail.com">shpe.gastate@gmail.com</a>
              </p>
            </div>

            <a href="/#/" className="back-home-btn secondary">
              Back to Home
            </a>
          </div>
        )}
      </div>
    </div>
  );
}

export default ConfirmAttendance;