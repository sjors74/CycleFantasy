window.attachPasswordChecker = function (inputId) {
    const passwordInput = document.getElementById(inputId);
    if (!passwordInput) return;

    passwordInput.addEventListener("input", function () {
        const value = passwordInput.value;

        const hasLength = value.length >= 8;
        const hasUppercase = /[A-Z]/.test(value);
        const hasNumber = /[0-9]/.test(value);
        const hasSpecialChar = /[!@#$%^&*(),.?":{}|<>]/.test(value);

        toggleRule("length-rule", hasLength);
        toggleRule("uppercase-rule", hasUppercase);
        toggleRule("number-rule", hasNumber);
        toggleRule("specialchar-rule", hasSpecialChar);
    });

    function toggleRule(ruleId, conditionMet) {
        const el = document.getElementById(ruleId);
        if (!el) return;

        const icon = el.querySelector(".icon");
        if (icon) {
            icon.textContent = conditionMet ? "✅" : "❌";
        }

        el.classList.toggle("valid", conditionMet);
        el.classList.toggle("invalid", !conditionMet);
    }
};