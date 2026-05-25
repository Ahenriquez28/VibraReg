import { useState, useEffect } from "react";
import "./RegisterForm.css";
import WhiteCity from '../../public/WhiteCity.png';
import type { RegisterDTO } from "../models/RegisterDTO";

const gpaOptions = [
  "4.0",
  "3.5 - 3.99",
  "3.0 - 3.49",
  "2.5 - 2.99",
  "2.0 - 2.49",
  "Below 2.0",
];

const schoolOptions = [
  "Georgia State",
  "Georgia Tech",
  "Kennesaw State",
  "University of Georgia",
  "Georgia Gwinnett College",
  "Berry College",
  "Other"
];

function RegisterForm() {
  const [hasGroup, setHasGroup] = useState<"yes" | "no">("no");
  const [resumeFile, setResumeFile] = useState<File | null>(null);
  const [loading, setLoading] = useState(false);
  const [showSuccessPopup, setShowSuccessPopup] = useState(false);
  const [showErrorPopup, setShowErrorPopup] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");
  const [selectedSchool, setSelectedSchool] = useState("");
  const [availableTeams, setAvailableTeams] = useState<string[]>([]);
  const [selectedTeam, setSelectedTeam] = useState("");

  useEffect(() => {
    // Fetch available team names when component mounts
    const fetchTeamNames = async () => {
      try {
        const API_BASE_URL = import.meta.env.VITE_API_URL;
        const response = await fetch(`${API_BASE_URL}/team-names`);
        const data = await response.json();
        if (data.success && data.teamNames) {
          setAvailableTeams(data.teamNames);
        }
      } catch (error) {
        console.error('Failed to fetch team names:', error);
      }
    };

    fetchTeamNames();
  }, []);

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setLoading(true);

    const form = e.currentTarget;
    const formData = new FormData(form);

    const hasGroupValue = (formData.get("group") as string) === "yes";
    
    // Combine first and last name
    const firstName = formData.get("firstName") as string;
    const lastName = formData.get("lastName") as string;
    const fullName = `${firstName} ${lastName}`;

    // Get school value - either from dropdown or custom input
    const schoolValue = selectedSchool === "Other" 
      ? (formData.get("otherSchool") as string)
      : selectedSchool;

    // Get group name - either from dropdown or custom input
    let cleanedGroupName: string | undefined = undefined;
    if (hasGroupValue) {
      if (selectedTeam === "custom") {
        const customTeam = formData.get("customTeamName") as string;
        cleanedGroupName = customTeam ? customTeam.trim().toLowerCase() : undefined;
      } else {
        cleanedGroupName = selectedTeam ? selectedTeam.trim().toLowerCase() : undefined;
      }
    }

    const dto: RegisterDTO = {
      fullName: fullName,
      email: formData.get("email") as string,
      school: schoolValue,
      gpa: formData.get("gpa") as string,
      hasGroup: hasGroupValue,
      groupName: cleanedGroupName,
      resume: resumeFile ?? undefined,
    };

    try {
      const uploadData = new FormData();

      Object.entries(dto).forEach(([key, value]) => {
        if (value !== undefined) {
          const capitalizedKey = key.charAt(0).toUpperCase() + key.slice(1);
          
          if (key === "resume" && value instanceof File) {
            uploadData.append(capitalizedKey, value);
          } else if (Array.isArray(value)) {
            uploadData.append(capitalizedKey, value.join(","));
          } else {
            uploadData.append(capitalizedKey, value.toString());
          }
        }
      });

      const API_BASE_URL = import.meta.env.VITE_API_URL;
      const endpoint = `${API_BASE_URL}/registration`;

      const res = await fetch(endpoint, { method: "POST", body: uploadData });

      if (res.ok) {
        setShowSuccessPopup(true);
        form.reset();
        setHasGroup("no");
        setResumeFile(null);
        setSelectedSchool("");
        setSelectedTeam("");
      } else {
        const errorData = await res.json();  
        console.log("Full error from API:", JSON.stringify(errorData, null, 2));
        
        const userMessage = errorData.message || "Failed to submit registration. Please try again.";
        setErrorMessage(userMessage);
        setShowErrorPopup(true);
      }
    } catch {
      setErrorMessage("Network error. Please try again.");
      setShowErrorPopup(true);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="register-page">
      <h1 className="register-title">Register for Hackathon</h1>

      <form className="register-form" onSubmit={handleSubmit}>
        <input name="firstName" type="text" placeholder="First Name" required />
        <input name="lastName" type="text" placeholder="Last Name" required />
        <input name="email" type="email" placeholder="School Email" required />
        
        <select 
          name="school" 
          value={selectedSchool}
          onChange={(e) => setSelectedSchool(e.target.value)}
          required
        >
          <option value="">Select School</option>
          {schoolOptions.map((school) => (
            <option key={school} value={school}>{school}</option>
          ))}
        </select>

        {selectedSchool === "Other" && (
          <input 
            name="otherSchool" 
            type="text" 
            placeholder="Enter your school name" 
            required 
          />
        )}

        <select name="gpa">
          <option value="">Select GPA (Optional)</option>
          {gpaOptions.map((gpa) => (
            <option key={gpa} value={gpa}>{gpa}</option>
          ))}
        </select>

        <div className="group-section">
          <label>Do you already have a group?</label>

          <div className="radio-group">
            <label>
              <input
                type="radio"
                name="group"
                value="no"
                checked={hasGroup === "no"}
                onChange={() => setHasGroup("no")}
              /> No
            </label>
            <label>
              <input
                type="radio"
                name="group"
                value="yes"
                checked={hasGroup === "yes"}
                onChange={() => setHasGroup("yes")}
              /> Yes
            </label>
          </div>

          {hasGroup === "yes" && (
            <>
              <p className="helper-text">
                Team capacity is up to 4 members.
              </p>
              
              <select
                value={selectedTeam}
                onChange={(e) => setSelectedTeam(e.target.value)}
                required={hasGroup === "yes"}
              >
                <option value="">Select existing team or create new</option>
                {availableTeams.map((team) => (
                  <option key={team} value={team}>
                    {team.replace(/!/g, '')} {/* Display without ! */}
                  </option>
                ))}
                <option value="custom">➕ Create New Team</option>
              </select>

              {selectedTeam === "custom" && (
                <input
                  name="customTeamName"
                  type="text"
                  placeholder="Enter new team name"
                  required
                  style={{ marginTop: '10px' }}
                />
              )}
            </>
          )}

          {hasGroup === "no" && (
            <p className="helper-text">
              If you don't have a group, our team will place you in a team!
            </p>
          )}
        </div>

        <div className="resume-section">
          <label>Upload Resume (Optional - PDF, PNG, JPG)</label>
          <input
            type="file"
            accept=".pdf,.png,.jpg,.jpeg"
            onChange={(e) => setResumeFile(e.target.files ? e.target.files[0] : null)}
          />
        </div>

        <button type="submit" disabled={loading}>
          {loading ? "Submitting..." : "Submit Registration"}
        </button>
      </form>

      {/* Success Popup */}
      {showSuccessPopup && (
        <div className="popup-overlay" onClick={() => setShowSuccessPopup(false)}>
          <div className="popup-content" onClick={(e) => e.stopPropagation()}>
            <h2>Registration Successful</h2>
            <p>Your registration has been submitted successfully.</p>
            <button onClick={() => setShowSuccessPopup(false)}>Close</button>
          </div>
        </div>
      )}

      {/* Error Popup */}
      {showErrorPopup && (
        <div className="popup-overlay" onClick={() => setShowErrorPopup(false)}>
          <div className="popup-content" onClick={(e) => e.stopPropagation()}>
            <h2>Error</h2>
            <p>{errorMessage}</p>
            <button onClick={() => setShowErrorPopup(false)}>Close</button>
          </div>
        </div>
      )}

      <img src={WhiteCity} alt="Corner" className="WhiteCity" />
    </div>
  );
}

export default RegisterForm;