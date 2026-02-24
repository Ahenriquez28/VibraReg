import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import './index.css';

import { HashRouter, Routes, Route, Navigate } from 'react-router-dom';
import InitalView from './HomePage/InitalView.tsx';
import ReasonsForJoin from './HomePage/Block_2.tsx';
import LocationInfo from './HomePage/Block_3.tsx';
import SponsorsInfo from './HomePage/Block_4.tsx';
import RegisterForm from './RegisterPage/RegisterForm.tsx';
import Admin from './AdminPage/Admin.tsx';
import Login from './LoginPage/Login.tsx';
import ProtectedRoute from './components/ProtectedRoute.tsx';
import ConfirmAttendance from './ConfirmationPage/ConfirmAttendance.tsx';


const AppRoutes = () => (
  <Routes>
    {/* Home page showing all sections */}
    <Route
      path="/"
      element={
        <>
          <InitalView />
          <ReasonsForJoin />
          <LocationInfo />
          <SponsorsInfo />
        </>
      }
    />
    {/* Register page */}
    <Route path="/register" element={<RegisterForm />} />
    
    {/* Login page */}
    <Route path="/login" element={<Login />} />
    
    
    <Route path="/confirm/:token" element={<ConfirmAttendance />} />

    
    {/* Admin page - protected */}
    <Route 
      path="/admin" 
      element={
        <ProtectedRoute>
          <Admin />
        </ProtectedRoute>
      } 
    />
  </Routes>
);

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <HashRouter>
      <AppRoutes />
    </HashRouter>
  </StrictMode>
);