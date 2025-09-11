$(document).ready(function () {

    $("ul.tabs a").removeClass("selected");
    $(".roles").addClass("selected");

    showServerMessageResult("#msg-list-roles");

    $('#frmEditor').on('submit', function (e, data) {
        e.preventDefault();
        $(".SelectedPermissions").remove();
        var selectedCheckboxes = $('.selected-permissions:checked').each(function (i, e) {
            var data = $(e).val().replace('chk_permission_', '').split('-');
            if (data.length > 0) {
                $('#frmEditor').append($('<input type="hidden" name="SelectedPermissions[' + i + '].PermissionId" value="' + data[0] + '" class="SelectedPermissions" />'));
            }
            if (data.length > 1) {
                $('#frmEditor').append($('<input type="hidden" name="SelectedPermissions[' + i + '].StatusId" value="' + data[1] + '" class="SelectedPermissions" />'));
            }
        });
        $.post($('#frmEditor').attr('action'), $('#frmEditor').serialize(), function (data) {
            showResultMessage(data);
        })
            .fail(function () {
                showMessage("#msg-list-roles", "Error al ejecutar la acción", 'error', false);
            });
        return false;
    });

    var showResultMessage = function (data) {
        $("html, body, .content").animate({ scrollTop: 0 }, "fast");
        if ('undefined' != data['success']) {
            showMessage("#msg-list-roles", data['success'], 'confirmation', true);
        } else {
            var message = 'undefined' != data['error'] ? data['error'] : "Error al procesar el resultado";
            showMessage("#msg-list-roles", message, 'error', false);
        }
    }

    /**
     * Selected permissions handler functions
     */
    var selectedPermissionsHandler = function () {
        var permissions = new Array();

        var procPermission = function (element) {
            var permissionId = $(element).attr('data-permissionId');
            var statusId = $(element).attr('data-statusId');
            var label = $(element).attr('data-label');
            var code = $(element).attr('data-permissionCode');
            if ($(element).prop('checked') != true) {
                removeTagFor(permissionId, statusId, code);
            } else {
                addTagFor(permissionId, statusId, label, code);
            }
        }

        var parseTagId = function (permissionId, statusId, with_hash) {
            if ('undefined' === typeof (with_hash)) with_hash = true;
            var tag_id = (with_hash ? "#" : "") + "tag_permission";
            if (permissionId) tag_id = tag_id + "_" + permissionId;
            if (statusId) tag_id = tag_id + "_" + statusId;
            return tag_id;
        }

        var parseCheckId = function (permissionId, statusId, with_hash) {
            if ('undefined' === typeof (with_hash)) with_hash = true;
            var check_id = (with_hash ? "#" : "") + "chk_permission";
            if (permissionId) check_id = check_id + "_" + permissionId;
            if (statusId != "") check_id = check_id + "-" + statusId;
            return check_id;
        }

        var parseTagGroupId = function (permissionCode, with_hash) {
            if ('undefined' === typeof (with_hash)) with_hash = true;
            var tag_id = (with_hash ? "#" : "") + "tag_group_";
            var name = permissionCode.split('_');
            return tag_id + name[0].toLowerCase();
        }

        var parseTagGroupLabel = function (permissionCode) {
            var name = permissionCode.split(' - ');
            return name[0];
        }

        var removeTagGroup = function (groupCode) {
            $(groupCode).remove();
        }

        var removeTagFor = function (permissionId, statusId, permissionCode) {
            $(parseTagId(permissionId, statusId)).remove();
            if ($(parseTagGroupId(permissionCode) + ' .perm-group-list .perm-label').length == 0) {
                $(parseTagGroupId(permissionCode)).remove();
            }
        }

        var addGroupTag = function (groupCode, label) {
            var group = $('<div class="perm-group" id="' + groupCode + '"><div class="perm-group-label">' + label + '</div><div class="perm-group-list"></div></div>');
            $('#label-list').append(group);
        }

        var addTagFor = function (permissionId, statusId, label, permissionCode) {
            if ($(parseTagGroupId(permissionCode)).length < 1) {
                addGroupTag(parseTagGroupId(permissionCode, false), parseTagGroupLabel(label));
            }
            var label_parts = label.split(' - ');
            if (label_parts.length > 1) label_parts.splice(0, 1);
            var link = $('<div class="perm-label" id="' + parseTagId(permissionId, statusId, false) + '" href="#" data-permissionId="' + permissionId + '" data-statusId="' + statusId + '">' + label_parts.join(' - ') + '</div>');
            $(parseTagGroupId(permissionCode) + ' .perm-group-list').append(link);
            link.on('click', function (e) {
                var permissionId = $(this).attr('data-permissionId');
                var statusId = $(this).attr('data-statusId');
                var id = parseCheckId(permissionId, statusId);
                $(id).prop('checked', false);
                procPermission($(id));
            });
        }

        // first update all
        $('.selected-permissions').each(function (i, e) {
            if ($(e).prop('checked'))
                procPermission(e);
        });

        $('.selected-permissions').on('change', function (e) {
            procPermission(this);
        });

    }

    selectedPermissionsHandler();

    /**
     * Table filterer
     * @param string table_selector
     * @param string search_input_selector
     */
    var initTableSearch = function (table_selector, search_input_selector) {
        // Filter aplications
        $(table_selector + " tbody tr:has(td.index)").each(function () {
            var t = $(this).text().toLowerCase();
            $("<td class='indexColumn'></td>").hide().text(t).appendTo(this);
        });
        $("<p align='center'>No se encontraron resultados</p>").hide().insertAfter($(table_selector));
        $(search_input_selector).on('keyup', function () {
            var s = $(this).val().toLowerCase();
            var dfd = new $.Deferred();
            dfd.then(function () {
                $(table_selector + " tbody tr:visible .indexColumn:not(:contains('" + s + "'))").parent().hide();
            })
                .then(function () {
                    $('.perm-label').removeClass('filtered');
                })
                .then(function () {
                    $(table_selector + " tbody tr:hidden .indexColumn:contains('" + s + "')").parent().show();
                })
                .then(function () {
                    if ($(table_selector + ' tbody tr:visible').length > 0) {
                        $(table_selector).next().hide();
                    } else {
                        $(table_selector).next().show();
                    }
                })
                .then(function () {
                    $('.perm-label').each(function () {
                        console.log($(this).text().toLowerCase().indexOf(s));
                        if ($(this).text().toLowerCase().indexOf(s) > -1) {
                            $(this).addClass('filtered');
                        }
                    })
                });
            dfd.resolve();
        });
    }

    initTableSearch('#list_perm', '#list_perm_search');
});