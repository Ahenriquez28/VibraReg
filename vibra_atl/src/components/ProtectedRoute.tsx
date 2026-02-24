import { Navigate } from 'react-router-dom';

interface ProtectedRouteProps {
  children: React.ReactNode;
}

function ProtectedRoute({ children }: ProtectedRouteProps) {
  const token = localStorage.getItem('authToken');
  const expiry = localStorage.getItem('tokenExpiry');

  // Check if token exists
  if (!token) {
    return <Navigate to="/login" replace />;
  }

  // Check if token is expired
  if (expiry && new Date(expiry) < new Date()) {
    localStorage.clear();
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
}

export default ProtectedRoute;