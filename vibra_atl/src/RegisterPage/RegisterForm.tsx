import { useState } from "react";
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

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setLoading(true);
    console.log("Form submitted! hasGroup =", hasGroup); 

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

    const rawGroupName = formData.get("groupName") as string | null;
    const cleanedGroupName = rawGroupName && hasGroupValue ? rawGroupName.trim().toLowerCase() : undefined;

    const dto: RegisterDTO = {
      fullName: fullName,
      email: formData.get("email") as string,
      school: schoolValue,
      gpa: formData.get("gpa") as string,
      hasGroup: hasGroupValue,
      groupName: cleanedGroupName,
      resume: resumeFile ?? undefined, // Resume is optional - undefined if not uploaded
    };

    try {
      const uploadData = new FormData();

      Object.entries(dto).forEach(([key, value]) => {
        if (value !== undefined) {
          // Capitalize first letter to match C# conventions (FullName, Email, etc.)
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
      } else {
        const errorData = await res.json();  
        console.log("Full error from API:", JSON.stringify(errorData, null, 2));  // ✅ Changed this line
        setErrorMessage("Failed to submit registration. Please try again.");
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
              <input
                name="groupName"
                type="text"
                placeholder="Group Name"
                required={hasGroup === "yes"}
                style={{ display: hasGroup === "yes" ? "block" : "none" }}
              />
            </>
          )}
        </div>

        <div className="resume-section">
          <label>Upload Resume (Optional - PDF, PNG, JPG)</label>
          <input
            type="file"
            accept=".pdf,.png,.jpg,.jpeg"
            onChange={(e) => setResumeFile(e.target.files ? e.target.files[0] : null)}
            // No 'required' attribute - making it optional
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