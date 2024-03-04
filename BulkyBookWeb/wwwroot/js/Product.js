$(document).ready(
    function () {
        loadDataTable();
    }
);

function loadDataTable() {
    DataTable = $('#tblData').DataTable({
        ajax: { url:'/admin/product/getall' },
        columns: [
            { data: 'title', "width": "25%" },
            { data: 'isbn', "width": "15%" },
            { data: 'price', "width": "10%" },
            { data: 'author', "width": "20%" },
            { data: 'category.name', "width": "15%" },
        ]
    });
}