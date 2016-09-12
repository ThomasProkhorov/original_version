/*
 * Kendo UI Localization Project for v2013.2.716
 * Copyright 2013 Telerik AD. All rights reserved.
 *
 * Hebrew (he-IL) Language Pack
 *
 * Kendo UI home : http://uco.co.il
 * Author        : VI, UCO team
 *
 * This project is released to the public domain, although one must abide to the
 * licensing terms set forth by Telerik to use Kendo UI, as shown bellow.
 *
 * Telerik's original licensing terms:
 * -----------------------------------
 * Kendo UI Web commercial licenses may be obtained at
 * https://www.kendoui.com/purchase/license-agreement/kendo-ui-web-commercial.aspx
 * If you do not own a commercial license, this file shall be governed by the
 * GNU General Public License (GPL) version 3.
 * For GPL requirements, please review: http://www.gnu.org/copyleft/gpl.html
 */

kendo.ui.Locale = "Hebrew (he-IL)";
kendo.ui.ColumnMenu.prototype.options.messages =
    $.extend(kendo.ui.ColumnMenu.prototype.options.messages, {

        /* COLUMN MENU MESSAGES
         ****************************************************************************/
        sortAscending: "עולה",
        sortDescending: "יורד",
        filter: "מסנן",
        columns: "רמקולים"
        /***************************************************************************/
    });

kendo.ui.Groupable.prototype.options.messages =
    $.extend(kendo.ui.Groupable.prototype.options.messages, {

        /* GRID GROUP PANEL MESSAGES
         ****************************************************************************/
        empty: "גרור כותרת עמודה לקבוצה על זה"
        /***************************************************************************/
    });
/* Grid messages */


    kendo.ui.Grid.prototype.options.messages =
    $.extend(true, kendo.ui.Grid.prototype.options.messages, {
        "commands": {
            "create": "הוסף",
            "destroy": "Удалить",
            "canceledit": "Отмена",
            "update": "Обновить",
            "edit": "Изменить",
            "select": "Выбрать",
            "cancel": "Отменить изменения",
            "save": "Сохранить изменения"
        },
        "editable": {
            "confirmation": "Вы уверены, что хотите удалить эту запись?",
            "cancelDelete": "Отмена",
            "confirmDelete": "Удалить"
        }
    });




kendo.ui.FilterMenu.prototype.options.messages =
    $.extend(kendo.ui.FilterMenu.prototype.options.messages, {

        /* FILTER MENU MESSAGES
         ***************************************************************************/
        info: "מסנן:",        // sets the text on top of the filter menu
        filter: "החל",      // sets the text for the "Filter" button
        clear: "לבטל",        // sets the text for the "Clear" button
        // when filtering boolean numbers
        isTrue: "ש", // sets the text for "isTrue" radio button
        isFalse: "לא",     // sets the text for "isFalse" radio button
        //changes the text of the "And" and "Or" of the filter menu
        and: "ו",
        or: "או",
        selectValue: "-בחר-"
        /***************************************************************************/
    });

kendo.ui.FilterMenu.prototype.options.operators =
    $.extend(kendo.ui.FilterMenu.prototype.options.operators, {

        /* FILTER MENU OPERATORS (for each supported data type)
         ****************************************************************************/
        string: {
            eq: "שם מדוייק",
            neq: "לא שווה",
            startswith: "מתחיל עם",
            contains: "מכיל",
            doesnotcontain: "אינו מכיל",
            endswith: "מסתיים ב"
        },
        number: {
            eq: "שם מדוייק",
            neq: "לא שווה",
            gte: "גדול או שווה",
            gt: "יותר",
            lte: "פחות או שווה",
            lt: "פחות"
        },
        date: {
            eq: "שם מדוייק",
            neq: "לא שווה",
            gte: "גדול או שווה",
            gt: "מאוחר יותר",
            lte: "פחות או שווה",
            lt: "לפני"
        },
        enums: {
            eq: "שם מדוייק",
            neq: "לא שווה"
        }
        /***************************************************************************/
    });

kendo.ui.Pager.prototype.options.messages =
    $.extend(kendo.ui.Pager.prototype.options.messages, {

        /* PAGER MESSAGES
         ****************************************************************************/
        display: "{0} - {1} מ {2} רשומות",
        empty: "אין מידע",
        page: "דף",
        of: "מ {0}",
        itemsPerPage: "רשומות בכל עמוד",
        first: "עמוד ראשון",
        previous: "קודם",
        next: "הבא",
        last: "הדף האחרון",
        refresh: "רענן"
        /***************************************************************************/
    });

kendo.ui.Validator.prototype.options.messages =
    $.extend(kendo.ui.Validator.prototype.options.messages, {

        /* VALIDATOR MESSAGES
         ****************************************************************************/
        required: "{0} נדרש",
        pattern: "{0} לא נכון",
        min: "{0} חייב להיות גדול או שווה ל {1}",
        max: "{0} חייב להיות קטן או שווה ל {1}",
        step: "{0} לא נכון",
        email: "{0} לא דואר אלקטרוני נכון",
        url: "{0} לא כתובת אתר חוקית",
        date: "{0} לא התאריך הנכון"
        /***************************************************************************/
    });

kendo.ui.ImageBrowser.prototype.options.messages =
    $.extend(kendo.ui.ImageBrowser.prototype.options.messages, {

        /* IMAGE BROWSER MESSAGES
         ****************************************************************************/
        uploadFile: "הורדה",
        orderBy: "מיין לפי",
        orderByName: "שם פרטים",
        orderBySize: "גודל",
        directoryNotFound: "ספרייה עם השם שצוין אינה קיימת",
        emptyFolder: "המדריך ריק",
        deleteFile: 'האם אתה בטוח שברצונך למחוק "{0}"?',
        invalidFileType: "הקובץ שנבחר \"{0}\" אינו נתמך. סוגים זמינים {1}.",
        overwriteFile: "קובץ \"{0}\" כבר קיים. תחליף?",
        dropFilesHere: "גרור לכאן כדי להוריד קבצים"
        /***************************************************************************/
    });

/*
file Upload localization
*/

kendo.ui.Upload.prototype.options.localization =
$.extend(kendo.ui.Upload.prototype.options.localization, {
    cancel: "לבטל",
    dropFilesHere: "שחרר קבצים כאן כדי להעלות",
    headerStatusUploaded: "עָשׂוּי",
    headerStatusUploading: "העלאה...",
    remove: "הסר",
    retry: "נסה שוב",
    select: "בחרו קבצים...",
    statusFailed: "נכשל",
    statusUploaded: "נטען",
    statusUploading: "העלאה",
    statusWarning: "אזהרה",
    uploadSelectedFiles: "להעלות קבצים"
    });
kendo.ui.Editor.prototype.options.messages =
    $.extend(kendo.ui.Editor.prototype.options.messages, {

        /* EDITOR MESSAGES
         ****************************************************************************/
        bold: "נועז",
        italic: "נטוי",
        underline: "קו תחתון",
        strikethrough: "חוצה",
        superscript: "כִּתוּב מְעַל לָקַו",
        subscript: "צִיוּן",
        justifyCenter: "במרכז",
        justifyLeft: "בקצה השמאלי",
        justifyRight: "יישור לימין",
        justifyFull: "ברוחב",
        insertUnorderedList: "הכנס רשימה עם תבליטים",
        insertOrderedList: "הכנס רשימה ממוספרת",
        indent: "הגדלהזחה",
        outdent: "קטן כניסה",
        createLink: "הכנס קישור",
        unlink: "הסר היפר-קישור",
        insertImage: "הכנס תמונה",
        createTable: "הכנס טבלה",
        addRowAbove: "הוספת שורה מלמעלה",
        addRowBelow: "הוסף שורה מתחת",
        addColumnLeft: "הוסף עמודה משמאל",
        addColumnRight: "הוסף עמודה מהימין",
        deleteRow: "מחק שורה",
        deleteColumn: "מחק עמודה",
        viewHtml: "צפה בHTML",
        insertHtml: "הדבק את HTML",
        fontName: "גופן",
        fontNameInherit: "(לרשת גופן)",
        fontSize: "גודל גופן",
        fontSizeInherit: "(לרשת גודל)",
        formatting: "עיצוב",
        foreColor: "צבע גופן",
        backColor: "צבע רקע",
        style: "סגנון",
        emptyFolder: "ספרייה ריקה",
        uploadFile: "העלאת קובץ ל",
        orderBy: "מיין לפי:",
        orderBySize: "גודל",
        orderByName: "שם פרטים",
        invalidFileType: "הקובץ שנבחר \"{0}\" אינו נתמך. סוגים זמינים {1}.",
        overwriteFile: "קובץ \"{0}\" כבר קיים. תחליף?",
        deleteFile: 'האם אתה בטוח שברצונך למחוק "{0}"?',
        directoryNotFound: "ספרייה עם השם שצוין אינה קיימת",
        imageWebAddress: "כתובת אינטרנט",
        imageAltText: "טקסט חלופי",
        dialogInsert: "הכנס",
        dialogUpdate: "רענן",
        dialogButtonSeparator: "או",
        dialogCancel: "לבטל",
        linkWebAddress: "כתובת אינטרנט",
        linkText: "txt",
        linkToolTip: "Tooltip",
        linkOpenInNewWindow: "פתח קישור בחלון חדש"
        /***************************************************************************/
    });
