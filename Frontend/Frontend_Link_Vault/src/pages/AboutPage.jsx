import { Link } from "react-router-dom";

function AboutPage() {
  return (
    <div className="legal-page">
      <Link to="/" className="legal-back-link">
        ← Back to Vault
      </Link>

      <h1>About Video Link Vault</h1>

      <p>
        Video Link Vault is a personal project built by Timothy Eckart as the
        capstone for the Microsoft Software & Systems Academy (MSSA) Cloud
        Application Development track. It exists to solve one small, real
        problem: video links saved from TikTok, YouTube, Instagram, and Facebook
        end up scattered across four different apps, with no easy way to browse,
        tag, or find them again later. Video Link Vault is one place to save
        them instead, with search, categories, and tags.
      </p>

      <p>
        This is an independent, non-commercial project. It is not affiliated
        with, endorsed by, or sponsored by TikTok, YouTube, Google, Instagram,
        Facebook, or Meta — it links to content on those platforms but doesn't
        host or control it.
      </p>

      <p>
        Questions, feedback, or an issue to report?{" "}
        <a href="mailto:[contact email]">[contact email]</a>
      </p>
    </div>
  );
}

export default AboutPage;
