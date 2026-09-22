import { BrowserRouter, Routes, Route } from "react-router-dom";
import VaultPage from "./pages/VaultPage.jsx";

function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<VaultPage />} />
      </Routes>
    </BrowserRouter>
  );
}

export default AppRouter;
