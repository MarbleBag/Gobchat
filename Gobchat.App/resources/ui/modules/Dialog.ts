const templateDialog = 
`
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
`

export interface DialogOptions {
    title?: string
    dialogType?: "yesno" | "ok" | "okcancel" | "yes"
    dialogIcon?: "" | "warning"
    dialogText?: string
    dialogContent?: HTMLElement | JQuery
    buttons?: { [buttonText: string]: number }
    modal?: boolean
    autoOpen?: boolean    
    localized?: boolean
    height?: "auto" | number
    width?: "auto" | number
    resizable?: boolean
}


export function showErrorDialog(options: DialogOptions): Promise<number> {
    const nonOptionalOptions: DialogOptions = {
        title: "config.main.dialog.title.error",
        dialogType: "ok",
        dialogIcon: "warning"
    }
    return _showMessageDialog(options, nonOptionalOptions)
}

export function showConfirmationDialog(options: DialogOptions): Promise<number> {
    const nonOptionalOptions: DialogOptions = {
        title: "config.main.dialog.title.confirm",
        dialogType: "yesno",
        dialogIcon: "warning"
    }
    return _showMessageDialog(options, nonOptionalOptions)
}

export function showMessageDialog(options: DialogOptions): Promise<number> {
    return _showMessageDialog(options, {})
}

interface JQueryUIDialogOptions {
    title?: string
    resizable?: boolean
    classes?: { [key: string]: string } // "ui-dialog": "ui-corner-all", "ui-dialog-titlebar": "ui-corner-all"
    height?: "auto" | number
    width?: "auto" | number
    modal?: boolean
    buttons?: { text: string, icon?: string, click: () => void }[]
    closeOnEscape?: boolean
    draggable?: boolean
}

const defaultUserOptions: DialogOptions = {
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
}

function _showMessageDialog(userOptions: DialogOptions, enforcedOptions: DialogOptions): Promise<number> {
    const mergedOptions: DialogOptions = { ...defaultUserOptions, ...userOptions, ...enforcedOptions }

    return new Promise<number>(async function (resolve, reject) {
        try {
            await processOptions(mergedOptions)

            const buttons = Object.keys(mergedOptions.buttons).map(text => {
                const value = mergedOptions.buttons[text]
                return {
                    text: text,
                    click: function () {
                        $(this).dialog("destroy").remove()
                        resolve(value)
                    }
                }
            })
      
            const $dialog = $(templateDialog)

            if (mergedOptions.dialogText.length > 0)
                $dialog.find("#dialog_content").append($("<span/>").html(mergedOptions.dialogText))

            if (mergedOptions.dialogContent)
                $dialog.find("#dialog_content").append($(mergedOptions.dialogContent))

            const jqueryDialogOptions: JQueryUIDialogOptions = {
                title: mergedOptions.title,
                modal: mergedOptions.modal,
                resizable: mergedOptions.resizable,
                height: mergedOptions.height,
                width: mergedOptions.width,
                classes: { "ui-dialog-titlebar": "ui-dialog-titlebar-close--hide" },
                closeOnEscape: false,
                buttons: buttons
            }

            $dialog.dialog(jqueryDialogOptions)

            switch (mergedOptions.dialogIcon) {
                case "warning":
                    $dialog.find("#icon_warning").show()
                    break;
            }

            $dialog.parent().find(".ui-dialog-titlebar-close").on("click", function () {
                $dialog.dialog("destroy").remove()
                resolve(0)
            })

            $dialog.dialog("open")
        } catch (e1) {
            console.error(e1)
            reject(e1)
        }
    })
}

async function processOptions(option: DialogOptions): Promise<void> {
    if (Object.keys(option.buttons).length == 0) {        
        switch (option.dialogType) {
            case "yesno":
                option.buttons = {
                    "config.main.dialog.btn.yes": 1,
                    "config.main.dialog.btn.no": 0
                }
                break;
            case "okcancel":
                option.buttons = {
                    "config.main.dialog.btn.ok": 1,
                    "config.main.dialog.btn.cancel": 0
                }
                break;
            case "yes":
                option.buttons = {
                    "config.main.dialog.btn.yes": 0
                }
                break;
            case "ok":
                option.buttons = {
                    "config.main.dialog.btn.ok": 0
                }
                break;
        }
    }

    if (option.localized) {
        const lookupKeys = [option.title].concat(Object.keys(option.buttons))
        if (option.dialogText.length > 0)
            lookupKeys.push(option.dialogText)

        const locales = await gobLocale.getAll(lookupKeys)
        option.title = locales[option.title]
        option.buttons = _.mapKeys(option.buttons, (v, k) => locales[k])

        if (option.dialogText.length > 0)
            option.dialogText = locales[option.dialogText]

        if (option.dialogContent)
            await gobLocale.updateElement(option.dialogContent)
    }
}