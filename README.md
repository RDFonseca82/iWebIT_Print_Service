# iWebIT_Print_Service


## Requisitos
- Windows 10/11/Server
- .NET 6 Desktop/Runtime (install the runtime on target machine)
- SumatraPDF (para imprimir PDFs silenciosamente) - instalar em `C:\Program Files\SumatraPDF\SumatraPDF.exe` ou ajustar `appsettings.json`.


## Build
1. Abrir o diretório do projeto no Visual Studio 2022/2023 (ou `dotnet build`).
2. Restaurar pacotes NuGet.
3. Build -> Release.


## Instalação (modo simples usando sc.exe)
1. Copiar `iWebIT_PrintAgent.exe` e `appsettings.json` para `C:\Program Files\iWebIT_PrintAgent\`.
2. Abra um prompt como Administrador e execute:


## API JSON

{
  "status": "ok",
  "jobs": [
    {
      "file_url": "https://seudominio.com/printjobs/12345.pdf",
      "printer_name": "HP LaserJet 400 M401",
      "job_id": 12345
    }
  ]
}


```powershell
sc create iWebIT_PrintAgent binPath= "\"C:\Program Files\iWebIT_PrintAgent\iWebIT_PrintAgent.exe\"" start= auto
sc description iWebIT_PrintAgent "iWebIT Print Agent - polls API and prints jobs"
sc start iWebIT_PrintAgent
```


## Notas importantes
- **Conta de serviço:** o serviço por defeito corre como LocalSystem. Para impressoras em rede que requerem credenciais, configure um user account com permissões e altere o logon do serviço.
- **Impressoras:** o nome de `printer_name` recebido no JSON tem de corresponder exatamente ao nome da impressora instalada no servidor/estação onde o serviço corre.
- **SumatraPDF:** recomendado para PDFs. Para outros tipos, o método `PrintTo` depende das associações de ficheiro instaladas e das aplicações presentes.
- **Debug:** verificar `C:\ProgramData\iWebIT_PrintAgent\log.txt`.


PHP API GetJobs
<?php
// getjobs.php
header('Content-Type: application/json');

$jobs = [
    [
        'file_url' => 'https://example.com/file1.jpg',
        'printer_name' => 'MinhaImpressora',
        'job_id' => '123'
    ],
    [
        'file_url' => 'https://example.com/file2.png',
        'printer_name' => 'MinhaImpressora',
        'job_id' => '124'
    ]
];

echo json_encode([
    'status' => 'ok',
    'jobs' => $jobs
]);
?>

PHP API Config Jobs

<?php
// confirm.php
$id = $_POST['id'] ?? null;
$status = $_POST['status'] ?? null;

if ($id && $status) {
    // Atualiza DB ou log do job
    echo "Job $id marcado como $status";
} else {
    echo "Dados inválidos";
}
?>