# Caminho para o PlantUML .jar
PLANTUML_JAR = /home/cardoso42/plantuml/plantuml-1.2024.4.jar

# Arquivos .puml da seção SRS
SRS_PUMLS := $(wildcard docs/srs/diagrams/*.puml)

# Alvo padrão — não faz nada
all:
	@echo "Use an explicit target, e.g.: make srs"

# Geração completa do SRS (diagramas + PDF)
srs: srs-diagrams srs-pdf clean

# Gera os diagramas do SRS
srs-diagrams:
	@echo "Gerando diagramas PlantUML..."
	java -jar $(PLANTUML_JAR) $(SRS_PUMLS)

# Compila o arquivo LaTeX do SRS
srs-pdf:
	@echo "Compilando LaTeX para PDF..."
	cd docs/srs && latexmk -pdf -interaction=nonstopmode main.tex

# Limpa os PNGs gerados em todas as subpastas diagrams
clean:
	@echo "Removendo arquivos auxiliares do LaTeX..."
	cd docs/srs && rm -f *.aux *.log *.out *.toc *.fls *.fdb_latexmk *.synctex.gz *.bbl *.blg

