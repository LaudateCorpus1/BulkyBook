var dataTable;

$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Company/GetAll",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "name", "width": "15%" },
            { "data": "streetAddress", "width": "15%" },
            { "data": "city", "width": "10%" },
            { "data": "state", "width": "10%" },
            { "data": "postalCode", "width": "10%" },
            { "data": "phoneNumber", "width": "10%" },
            {
                "data": "isAuthorizedCompany", "render": function (data) {
                    if (data)
                        return `<input type="checkbox" disabled checked /> `
                    else
                        return `<input type="checkbox" disabled /> `
                }, "width": "10%"
            },
            {
                "data": "id",
                "render": function (data) {
                    return `<div class="text-center">
                                <a href="Company/Upsert/${data}" class="btn-success" style="cursor:pointer">    <i class="fas fa-user-edit"></i> &nbsp;</a>
                                <a onclick=Delete("Company/Delete/${data}") class="btn-danger text-white" style="cursor:pointer">    <i class="fas fa-trash"></i> &nbsp;</a>
                            </div>`
                },
                "width": "20%"
            }
        ],
        "language": {
            "emptyTable": "No data found"
        },
        "width": "100%"
    })
}

function Delete(url) {
    swal({
        title: "Are you sure?",
        text: "Once deleted, you will not be able to recover.",
        icon: "warning",
        dangerMode: true,
        buttons: true
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                type: "DELETE",
                url: url,
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        dataTable.ajax.reload();
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    })
}