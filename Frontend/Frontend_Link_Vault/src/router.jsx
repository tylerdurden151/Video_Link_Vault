import { BrowserRouter, Routes, Route } from "react-router-dom";
import App from "./App.jsx";

function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<App />} />
      </Routes>
    </BrowserRouter>
  );
}

export default AppRouter;
