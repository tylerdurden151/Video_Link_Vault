import { Link } from "react-router-dom";

function Footer({ savedCount, shownCount }) {
  return (
    <footer className="app-footer">
      <div>
        <div>Video Link Vault</div>
        <div className="muted">
          {savedCount} saved · {shownCount} shown
        </div>
      </div>
      <nav className="footer-legal-links">
        <Link to="/about">About</Link>
        <Link to="/faq">FAQ</Link>
        <Link to="/privacy">Privacy Policy</Link>
        <Link to="/terms">Terms of Service</Link>
      </nav>
    </footer>
  );
}

export default Footer;
