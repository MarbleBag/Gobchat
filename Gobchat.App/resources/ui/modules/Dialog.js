const templateDialog = `
<div title="test">
    <p>
        <span style="float:left; margin:24px 24px 20px 0;">
            <span id="icon_warning" hidden>
                <i class="fas fa-exclamation-triangle fa-3x gob-icon-warning"></i>
            </span>
        </span>
        <span id="dialog_content"></span>
    </p>
</div>
`;
export function showErrorDialog(options) {
    const nonOptionalOptions = {
        title: "config.main.dialog.title.error",
        dialogType: "ok",
        dialogIcon: "warning"
    };
    return _showMessageDialog(options, nonOptionalOptions);
}
export function showConfirmationDialog(options) {
    const nonOptionalOptions = {
        title: "config.main.dialog.title.confirm",
        dialogType: "yesno",
        dialogIcon: "warning"
    };
    return _showMessageDialog(options, nonOptionalOptions);
}
export function showMessageDialog(options) {
    return _showMessageDialog(options, {});
}
const defaultUserOptions = {
    resizable: false,
    width: 600,
    modal: true,
    autoOpen: false,
    buttons: {},
    dialogType: "ok",
    dialogIcon: "",
    dialogText: "",
    dialogContent: null,
    localized: true
};
function _showMessageDialog(userOptions, enforcedOptions) {
    const mergedOptions = { ...defaultUserOptions, ...userOptions, ...enforcedOptions };
    return new Promise(async function (resolve, reject) {
        try {
            await processOptions(mergedOptions);
            const buttons = Object.keys(mergedOptions.buttons).map(text => {
                const value = mergedOptions.buttons[text];
                return {
                    text: text,
                    click: function () {
                        $(this).dialog("destroy").remove();
                        resolve(value);
                    }
                };
            });
            const $dialog = $(templateDialog);
            if (mergedOptions.dialogText.length > 0)
                $dialog.find("#dialog_content").append($("<span/>").html(mergedOptions.dialogText));
            if (mergedOptions.dialogContent)
                $dialog.find("#dialog_content").append($(mergedOptions.dialogContent));
            const jqueryDialogOptions = {
                title: mergedOptions.title,
                modal: mergedOptions.modal,
                resizable: mergedOptions.resizable,
                height: mergedOptions.height,
                width: mergedOptions.width,
                classes: { "ui-dialog-titlebar": "ui-dialog-titlebar-close--hide" },
                closeOnEscape: false,
                buttons: buttons
            };
            $dialog.dialog(jqueryDialogOptions);
            switch (mergedOptions.dialogIcon) {
                case "warning":
                    $dialog.find("#icon_warning").show();
                    break;
            }
            $dialog.parent().find(".ui-dialog-titlebar-close").on("click", function () {
                $dialog.dialog("destroy").remove();
                resolve(0);
            });
            $dialog.dialog("open");
        }
        catch (e1) {
            console.error(e1);
            reject(e1);
        }
    });
}
async function processOptions(option) {
    if (Object.keys(option.buttons).length == 0) {
        switch (option.dialogType) {
            case "yesno":
                option.buttons = {
                    "config.main.dialog.btn.yes": 1,
                    "config.main.dialog.btn.no": 0
                };
                break;
            case "okcancel":
                option.buttons = {
                    "config.main.dialog.btn.ok": 1,
                    "config.main.dialog.btn.cancel": 0
                };
                break;
            case "yes":
                option.buttons = {
                    "config.main.dialog.btn.yes": 0
                };
                break;
            case "ok":
                option.buttons = {
                    "config.main.dialog.btn.ok": 0
                };
                break;
        }
    }
    if (option.localized) {
        const lookupKeys = [option.title].concat(Object.keys(option.buttons));
        if (option.dialogText.length > 0)
            lookupKeys.push(option.dialogText);
        const locales = await gobLocale.getAll(lookupKeys);
        option.title = locales[option.title];
        option.buttons = _.mapKeys(option.buttons, (v, k) => locales[k]);
        if (option.dialogText.length > 0)
            option.dialogText = locales[option.dialogText];
        if (option.dialogContent)
            await gobLocale.updateElement(option.dialogContent);
    }
}
