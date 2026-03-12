using System.Collections.Generic;

[System.Serializable]
public class Capital
{
    public string country;
    public string capital;
    public string continent;

    public Capital(string country, string capital, string continent)
    {
        this.country = country;
        this.capital = capital;
        this.continent = continent;
    }
}

public static class CapitalDatabase
{
    public static List<Capital> All = new List<Capital>
    {
        // Europe
        new Capital("France", "Paris", "Europe"),
        new Capital("Allemagne", "Berlin", "Europe"),
        new Capital("Espagne", "Madrid", "Europe"),
        new Capital("Italie", "Rome", "Europe"),
        new Capital("Portugal", "Lisbonne", "Europe"),
        new Capital("Pays-Bas", "Amsterdam", "Europe"),
        new Capital("Belgique", "Bruxelles", "Europe"),
        new Capital("Suisse", "Berne", "Europe"),
        new Capital("Autriche", "Vienne", "Europe"),
        new Capital("Pologne", "Varsovie", "Europe"),
        new Capital("Suède", "Stockholm", "Europe"),
        new Capital("Norvège", "Oslo", "Europe"),
        new Capital("Danemark", "Copenhague", "Europe"),
        new Capital("Finlande", "Helsinki", "Europe"),
        new Capital("Grèce", "Athènes", "Europe"),
        new Capital("Turquie", "Ankara", "Europe"),
        new Capital("Roumanie", "Bucarest", "Europe"),
        new Capital("Hongrie", "Budapest", "Europe"),
        new Capital("République tchèque", "Prague", "Europe"),
        new Capital("Ukraine", "Kiev", "Europe"),
        new Capital("Russie", "Moscou", "Europe"),
        new Capital("Serbie", "Belgrade", "Europe"),
        new Capital("Croatie", "Zagreb", "Europe"),
        new Capital("Slovaquie", "Bratislava", "Europe"),
        new Capital("Bulgarie", "Sofia", "Europe"),

        // Amérique
        new Capital("États-Unis", "Washington D.C.", "Amérique"),
        new Capital("Canada", "Ottawa", "Amérique"),
        new Capital("Mexique", "Mexico", "Amérique"),
        new Capital("Brésil", "Brasília", "Amérique"),
        new Capital("Argentine", "Buenos Aires", "Amérique"),
        new Capital("Colombie", "Bogotá", "Amérique"),
        new Capital("Chili", "Santiago", "Amérique"),
        new Capital("Pérou", "Lima", "Amérique"),
        new Capital("Venezuela", "Caracas", "Amérique"),
        new Capital("Cuba", "La Havane", "Amérique"),
        new Capital("Bolivie", "Sucre", "Amérique"),
        new Capital("Uruguay", "Montevideo", "Amérique"),
        new Capital("Paraguay", "Asunción", "Amérique"),
        new Capital("Équateur", "Quito", "Amérique"),

        // Asie
        new Capital("Chine", "Pékin", "Asie"),
        new Capital("Japon", "Tokyo", "Asie"),
        new Capital("Inde", "New Delhi", "Asie"),
        new Capital("Corée du Sud", "Séoul", "Asie"),
        new Capital("Thaïlande", "Bangkok", "Asie"),
        new Capital("Vietnam", "Hanoï", "Asie"),
        new Capital("Indonésie", "Jakarta", "Asie"),
        new Capital("Pakistan", "Islamabad", "Asie"),
        new Capital("Bangladesh", "Dacca", "Asie"),
        new Capital("Philippines", "Manille", "Asie"),
        new Capital("Malaisie", "Kuala Lumpur", "Asie"),
        new Capital("Arabie saoudite", "Riyad", "Asie"),
        new Capital("Iran", "Téhéran", "Asie"),
        new Capital("Irak", "Bagdad", "Asie"),
        new Capital("Israël", "Jérusalem", "Asie"),
        new Capital("Jordanie", "Amman", "Asie"),
        new Capital("Kazakhstan", "Astana", "Asie"),
        new Capital("Afghanistan", "Kaboul", "Asie"),
        new Capital("Birmanie", "Naypyidaw", "Asie"),
        new Capital("Cambodge", "Phnom Penh", "Asie"),
        new Capital("Mongolie", "Oulan-Bator", "Asie"),

        // Afrique
        new Capital("Maroc", "Rabat", "Afrique"),
        new Capital("Algérie", "Alger", "Afrique"),
        new Capital("Tunisie", "Tunis", "Afrique"),
        new Capital("Égypte", "Le Caire", "Afrique"),
        new Capital("Éthiopie", "Addis-Abeba", "Afrique"),
        new Capital("Nigeria", "Abuja", "Afrique"),
        new Capital("Afrique du Sud", "Pretoria", "Afrique"),
        new Capital("Kenya", "Nairobi", "Afrique"),
        new Capital("Ghana", "Accra", "Afrique"),
        new Capital("Sénégal", "Dakar", "Afrique"),
        new Capital("Côte d'Ivoire", "Yamoussoukro", "Afrique"),
        new Capital("Cameroun", "Yaoundé", "Afrique"),
        new Capital("Mozambique", "Maputo", "Afrique"),
        new Capital("Madagascar", "Antananarivo", "Afrique"),
        new Capital("Tanzanie", "Dodoma", "Afrique"),
        new Capital("Ouganda", "Kampala", "Afrique"),
        new Capital("Zimbabwe", "Harare", "Afrique"),

        // Océanie
        new Capital("Australie", "Canberra", "Océanie"),
        new Capital("Nouvelle-Zélande", "Wellington", "Océanie"),
        new Capital("Papouasie-Nouvelle-Guinée", "Port Moresby", "Océanie"),
        new Capital("Fidji", "Suva", "Océanie"),
    };
}
