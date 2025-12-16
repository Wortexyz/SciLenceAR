// File: Assets/Scripts/FirebaseAuthManager.cs
// Updated: simplified Reset flow (format-only validation + friendly status messages),
// improved login error messages, internet checks, coroutine-safe returns.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine.SceneManagement;

public class FirebaseAuthManager : MonoBehaviour
{
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser user;
    private FirebaseFirestore db;

    [Space]
    [Header("Login")]
    public InputField emailLoginField;
    public InputField passwordLoginField;
    public Button loginButton;

    [Space]
    [Header("Registration")]
    public InputField nameRegisterField;
    public InputField emailRegisterField;
    public InputField passwordRegisterField;
    public InputField confirmPasswordRegisterField;
    public Button registerButton;

    [Space]
    [Header("Error & Status Texts")]
    public TMP_Text loginErrorText;
    public TMP_Text registerErrorText;
    public TMP_Text loginStatusText;
    public TMP_Text registerStatusText;

    [Header("Reset Password Settings")]
    public int resetCooldownSeconds = 60;

    [Header("Behaviour")]
    [Tooltip("If true loads a scene after sign-in. If false opens subject panel.")]
    public bool loadGameSceneAfterSignIn = false;
    public string gameSceneName = "GameScene";

    public int minPasswordLength = 6;

    void Start()
    {
        if (loginErrorText) loginErrorText.gameObject.SetActive(false);
        if (registerErrorText) registerErrorText.gameObject.SetActive(false);
        if (loginStatusText) loginStatusText.gameObject.SetActive(false);
        if (registerStatusText) registerStatusText.gameObject.SetActive(false);

        StartCoroutine(CheckAndFixDependenciesAsync());
    }

    private IEnumerator CheckAndFixDependenciesAsync()
    {
        var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(() => dependencyTask.IsCompleted);

        dependencyStatus = dependencyTask.Result;

        if (dependencyStatus == DependencyStatus.Available)
        {
            InitializeFirebase();
            yield return new WaitForEndOfFrame();
            StartCoroutine(CheckForAutoEntry());
        }
        else
        {
            Debug.LogError("Firebase dependency error: " + dependencyStatus);
            if (registerStatusText)
                StartCoroutine(ShowError(registerStatusText, "Firebase unavailable.", 5f));
        }
    }

    void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

        auth.StateChanged -= AuthStateChanged;
        auth.StateChanged += AuthStateChanged;

        user = auth.CurrentUser;
        AuthStateChanged(this, null);
    }

    private IEnumerator CheckForAutoEntry()
    {
        if (auth == null)
        {
            UIManager.Instance?.OpenHomePanel();
            yield break;
        }

        if (auth.CurrentUser != null)
        {
            user = auth.CurrentUser;
            var reloadTask = user.ReloadAsync();
            yield return new WaitUntil(() => reloadTask.IsCompleted);

            user = auth.CurrentUser;
            if (user != null)
            {
                GoToSubject();
                yield break;
            }
        }

        UIManager.Instance?.OpenHomePanel();
    }

    private void GoToSubject()
    {
        if (auth.CurrentUser != null)
            References.userName = auth.CurrentUser.DisplayName;

        if (loadGameSceneAfterSignIn)
            SceneManager.LoadScene(gameSceneName);
        else
            UIManager.Instance?.OpenSubjectPanel();

        var pm = FindObjectOfType<ProfileManager>();
        pm?.RefreshProfile();
    }

    private void AuthStateChanged(object sender, System.EventArgs e)
    {
        if (auth == null) return;

        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;

            if (!signedIn && user != null)
            {
                Debug.Log("Signed out: " + user.UserId);
                UIManager.Instance?.OpenHomePanel();
            }

            user = auth.CurrentUser;

            if (signedIn)
            {
                Debug.Log("Signed in: " + user.UserId);
                GoToSubject();
            }
        }
    }

    private void OnDestroy()
    {
        if (auth != null)
            auth.StateChanged -= AuthStateChanged;
    }

    // ------------------- LOGIN -------------------
    public void Login()
    {
        // If login panel exists but hidden, open it first
        if (UIManager.Instance != null &&
            UIManager.Instance.loginPanel != null &&
            !UIManager.Instance.loginPanel.activeSelf)
        {
            UIManager.Instance.homePanel?.SetActive(false);
            UIManager.Instance.loginPanel.SetActive(true);

            loginErrorText?.gameObject.SetActive(false);
            loginStatusText?.gameObject.SetActive(false);

            return;
        }

        // Internet check
        if (!IsOnline())
        {
            StartCoroutine(ShowError(loginErrorText, "No internet connection. Please enable the internet.", 3f));
            return;
        }

        string email = emailLoginField?.text.Trim();
        string password = passwordLoginField?.text;

        if (string.IsNullOrEmpty(email))
        {
            StartCoroutine(ShowError(loginErrorText, "Email is empty"));
            return;
        }
        if (!IsValidEmail(email))
        {
            StartCoroutine(ShowError(loginErrorText, "Invalid email format"));
            return;
        }
        if (string.IsNullOrEmpty(password))
        {
            StartCoroutine(ShowError(loginErrorText, "Password is empty"));
            return;
        }

        StartCoroutine(LoginAsync(email, password));
    }

    private IEnumerator LoginAsync(string email, string password)
    {
        SetButtonsInteractable(false);
        if (loginStatusText) { loginStatusText.text = "Signing in..."; loginStatusText.gameObject.SetActive(true); }

        var task = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => task.IsCompleted);

        SetButtonsInteractable(true);
        if (loginStatusText) loginStatusText.gameObject.SetActive(false);

        if (task.Exception != null)
        {
            FirebaseException fe = task.Exception.GetBaseException() as FirebaseException;
            string msg = "Login failed: ";

            if (fe != null)
            {
                var code = (AuthError)fe.ErrorCode;
                switch (code)
                {
                    case AuthError.InvalidEmail:
                        msg = "Invalid email address.";
                        break;
                    case AuthError.WrongPassword:
                        msg = "Incorrect password.";
                        break;
                    case AuthError.UserNotFound:
                        msg = "No account found with this email.";
                        break;
                    case AuthError.UserDisabled:
                        msg = "Account disabled. Contact support.";
                        break;
                    default:
                        // show the server message if available, otherwise a clean fallback
                        msg = !string.IsNullOrEmpty(fe.Message) ? fe.Message : "Unable to sign in. Please try again.";
                        break;
                }
            }
            else
            {
                msg = "Unable to sign in. Please check your credentials and try again.";
            }

            StartCoroutine(ShowError(loginErrorText, msg));
        }
        else
        {
            user = task.Result.User;
            References.userName = user.DisplayName;
            GoToSubject();
        }
    }

    // ------------------- REGISTER -------------------
    public void Register()
    {
        // If register panel exists but hidden, open it first
        if (UIManager.Instance != null &&
            UIManager.Instance.registerPanel != null &&
            !UIManager.Instance.registerPanel.activeSelf)
        {
            UIManager.Instance.homePanel?.SetActive(false);
            UIManager.Instance.registerPanel.SetActive(true);

            registerErrorText?.gameObject.SetActive(false);
            registerStatusText?.gameObject.SetActive(false);
            return;
        }

        // Internet check
        if (!IsOnline())
        {
            StartCoroutine(ShowError(registerErrorText, "No internet connection. Please enable the internet.", 3f));
            return;
        }

        StartCoroutine(RegisterAsync(
            nameRegisterField.text.Trim(),
            emailRegisterField.text.Trim(),
            passwordRegisterField.text,
            confirmPasswordRegisterField.text
        ));
    }

    private IEnumerator RegisterAsync(string name, string email, string password, string confirmPassword)
    {
        if (string.IsNullOrEmpty(name))
        {
            StartCoroutine(ShowError(registerErrorText, "Name is empty"));
            yield break;
        }
        if (!IsValidEmail(email))
        {
            StartCoroutine(ShowError(registerErrorText, "Invalid email format"));
            yield break;
        }
        if (password.Length < minPasswordLength)
        {
            StartCoroutine(ShowError(registerErrorText, $"Password must be at least {minPasswordLength} characters"));
            yield break;
        }
        if (password != confirmPassword)
        {
            StartCoroutine(ShowError(registerErrorText, "Passwords do not match"));
            yield break;
        }

        SetButtonsInteractable(false);
        if (registerStatusText) { registerStatusText.text = "Registering..."; registerStatusText.gameObject.SetActive(true); }

        var task = auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => task.IsCompleted);

        SetButtonsInteractable(true);
        if (registerStatusText) registerStatusText.gameObject.SetActive(false);

        if (task.Exception != null)
        {
            FirebaseException fe = task.Exception.GetBaseException() as FirebaseException;
            string msg = "Registration failed: ";
            if (fe != null)
            {
                switch ((AuthError)fe.ErrorCode)
                {
                    case AuthError.EmailAlreadyInUse: msg = "Email already in use."; break;
                    case AuthError.WeakPassword: msg = "Password too weak."; break;
                    default: msg = !string.IsNullOrEmpty(fe.Message) ? fe.Message : "Registration failed."; break;
                }
            }
            else
            {
                msg = "Registration failed. Please try again.";
            }

            StartCoroutine(ShowError(registerErrorText, msg));
            yield break;
        }

        user = task.Result.User;

        // Set display name on Firebase user
        var profileTask = user.UpdateUserProfileAsync(new UserProfile { DisplayName = name });
        yield return new WaitUntil(() => profileTask.IsCompleted);

        // Create user document in Firestore with name and email (so profile shows immediately)
        if (db != null)
        {
            var docRef = db.Collection("users").Document(user.UserId);
            var initial = new Dictionary<string, object>
            {
                { "name", name ?? "" },
                { "email", email ?? "" },
                { "createdAt", Timestamp.GetCurrentTimestamp() }
            };

            var setTask = docRef.SetAsync(initial);
            yield return new WaitUntil(() => setTask.IsCompleted);
            if (setTask.Exception != null)
            {
                Debug.LogWarning("[FirebaseAuthManager] Failed to create user doc: " + setTask.Exception);
            }
        }
        else
        {
            Debug.LogWarning("[FirebaseAuthManager] Firestore db is null - skipping user doc create.");
        }

        GoToSubject();
    }

    public void LogOut()
    {
        auth?.SignOut();

        var pm = FindObjectOfType<ProfileManager>();
        if (pm?.panelRoot != null) pm.panelRoot.SetActive(false);

        UIManager.Instance?.OpenHomePanel();
    }

    // ------------------------------------------------------
    // ------------------- RESET PASSWORD -------------------
    // ------------------------------------------------------

    public void SendPasswordReset()
    {
        // Open panel for reset
        if (UIManager.Instance != null &&
            UIManager.Instance.resetPasswordPanel != null)
        {
            UIManager.Instance.OpenResetPasswordPanelFromLogin();

            // Prefill with login email field (if available)
            if (UIManager.Instance.resetEmailField != null &&
                emailLoginField != null)
            {
                UIManager.Instance.resetEmailField.text =
                    emailLoginField.text.Trim();
            }

            return;
        }
    }

    // Called by Reset Panel Submit Button
    public void SubmitPasswordResetFromPanel()
    {
        if (UIManager.Instance == null) return;

        string email = (UIManager.Instance.resetEmailField != null) ? UIManager.Instance.resetEmailField.text.Trim() : string.Empty;

        if (string.IsNullOrEmpty(email))
        {
            if (UIManager.Instance.resetStatusText != null) UIManager.Instance.resetStatusText.text = "Enter your email.";
            return;
        }

        if (!IsValidEmail(email))
        {
            if (UIManager.Instance.resetStatusText != null) UIManager.Instance.resetStatusText.text = "Invalid email format.";
            return;
        }

        // Internet check
        if (!IsOnline())
        {
            if (UIManager.Instance.resetStatusText != null) UIManager.Instance.resetStatusText.text = "No internet connection. Please enable internet.";
            return;
        }

        // If user is signed in: require match with signed-in email
        if (auth != null && auth.CurrentUser != null)
        {
            string signedInEmail = auth.CurrentUser.Email ?? "";
            if (!string.Equals(email, signedInEmail, StringComparison.OrdinalIgnoreCase))
            {
                if (UIManager.Instance.resetStatusText != null) UIManager.Instance.resetStatusText.text = "Enter the email address used to sign in.";
                return;
            }

            // Signed in and matches -> send reset
            StartCoroutine(ResetWithFriendlySteps(email));
            return;
        }

        // Not signed in: do simple format-only flow (no provider check). Show friendly steps and send reset.
        StartCoroutine(ResetWithFriendlySteps(email));
    }

    // Friendly reset flow: show "Checking..." then "Sending..." then call SendPasswordResetEmailAsync.
    private IEnumerator ResetWithFriendlySteps(string email)
    {
        if (UIManager.Instance != null && UIManager.Instance.resetStatusText != null)
            UIManager.Instance.resetStatusText.text = "Checking email format...";

        // brief pause to simulate checking (UX)
        yield return new WaitForSeconds(0.6f);

        if (UIManager.Instance != null && UIManager.Instance.resetStatusText != null)
            UIManager.Instance.resetStatusText.text = "Sending reset request...";

        if (UIManager.Instance != null && UIManager.Instance.resetSubmitButton != null)
            UIManager.Instance.resetSubmitButton.interactable = false;

        var task = auth.SendPasswordResetEmailAsync(email);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            FirebaseException fe = task.Exception.GetBaseException() as FirebaseException;
            Debug.LogWarning("[FirebaseAuthManager] SendPasswordResetEmailAsync failed: " + (fe != null ? fe.Message : task.Exception.ToString()));
            if (UIManager.Instance != null && UIManager.Instance.resetStatusText != null)
                UIManager.Instance.resetStatusText.text = "Failed to send reset email. Please try again.";
            if (UIManager.Instance != null && UIManager.Instance.resetSubmitButton != null)
                UIManager.Instance.resetSubmitButton.interactable = true;
            yield break;
        }

        if (UIManager.Instance != null && UIManager.Instance.resetStatusText != null)
            UIManager.Instance.resetStatusText.text = "If an account exists, a reset link has been sent. Check your inbox.";

        // allow user to read the message
        yield return new WaitForSeconds(2f);

        UIManager.Instance?.CloseResetPasswordPanel();

        StartCoroutine(ResetCooldown());
    }

    private IEnumerator ResetCooldown()
    {
        if (UIManager.Instance == null || UIManager.Instance.resetSubmitButton == null) yield break;
        Button btn = UIManager.Instance.resetSubmitButton;
        btn.interactable = false;

        float t = 0;
        while (t < resetCooldownSeconds)
        {
            t += Time.deltaTime;
            yield return null;
        }

        btn.interactable = true;
    }

    // ------------------------------------------------------
    // Utility
    // ------------------------------------------------------

    private IEnumerator ShowError(TMP_Text t, string msg, float dur = 3f)
    {
        if (t == null) yield break;
        t.text = msg;
        t.gameObject.SetActive(true);
        yield return new WaitForSeconds(dur);
        t.gameObject.SetActive(false);
    }

    private void SetButtonsInteractable(bool state)
    {
        if (loginButton) loginButton.interactable = state;
        if (registerButton) registerButton.interactable = state;
    }

    private bool IsValidEmail(string email)
    {
        return Regex.IsMatch(email ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    private bool IsOnline()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }
}
